#!/usr/bin/env python3
"""
Shopizy GitHub Pull Request Review Agent powered by Google AI (Gemini).
Audits PR code diffs against Clean Architecture, Project Constitution, Security, and Test Completeness.
"""

import os
import sys
import json
import urllib.request
import urllib.error

def get_env_or_exit(name: str) -> str:
    val = os.environ.get(name)
    if not val:
        print(f"::error::Missing required environment variable: {name}")
        sys.exit(1)
    return val

def github_api_request(url: str, token: str, accept: str = "application/vnd.github.v3+json", method: str = "GET", data: dict = None) -> tuple[int, str]:
    headers = {
        "Authorization": f"Bearer {token}",
        "Accept": accept,
        "User-Agent": "Shopizy-PR-Review-Agent"
    }
    payload = json.dumps(data).encode("utf-8") if data else None
    req = urllib.request.Request(url, data=payload, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req) as resp:
            content = resp.read().decode("utf-8")
            return resp.status, content
    except urllib.error.HTTPError as e:
        error_content = e.read().decode("utf-8")
        print(f"::error::GitHub API call failed [{e.code}] {url}: {error_content}")
        return e.code, error_content

def discover_gemini_models(api_key: str) -> list[str]:
    url = f"https://generativelanguage.googleapis.com/v1beta/models?key={api_key}"
    req = urllib.request.Request(url, headers={"User-Agent": "Shopizy-PR-Review-Agent"})
    try:
        with urllib.request.urlopen(req) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            models = data.get("models", [])
            valid_models = []
            for m in models:
                methods = m.get("supportedGenerationMethods", [])
                if "generateContent" in methods:
                    name = m.get("name", "").replace("models/", "")
                    if name:
                        valid_models.append(name)
            print(f"Discovered {len(valid_models)} supported Gemini models: {valid_models[:5]}")
            return valid_models
    except Exception as e:
        print(f"::warning::Could not query ListModels: {e}")
        return []

def call_google_ai_gemini(prompt: str, api_key: str) -> str:
    discovered = discover_gemini_models(api_key)
    preferred = os.environ.get("GEMINI_MODEL", "gemini-3.6-flash")
    models_to_try = [preferred] + discovered + ["gemini-3.6-flash", "gemini-2.5-flash", "gemini-2.0-flash"]
    # Remove duplicates while preserving order
    models_to_try = list(dict.fromkeys(models_to_try))

    payload = {
        "contents": [
            {
                "parts": [
                    {"text": prompt}
                ]
            }
        ],
        "generationConfig": {
            "temperature": 0.2,
            "maxOutputTokens": 4096
        }
    }
    
    last_error = ""
    for model in models_to_try:
        url = f"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={api_key}"
        print(f"Calling Google AI Gemini model: {model}...")
        
        req = urllib.request.Request(
            url,
            data=json.dumps(payload).encode("utf-8"),
            headers={"Content-Type": "application/json"},
            method="POST"
        )
        
        try:
            with urllib.request.urlopen(req) as resp:
                data = json.loads(resp.read().decode("utf-8"))
                candidates = data.get("candidates", [])
                if candidates and "content" in candidates[0]:
                    parts = candidates[0]["content"].get("parts", [])
                    if parts and "text" in parts[0]:
                        return parts[0]["text"]
        except urllib.error.HTTPError as e:
            err = e.read().decode("utf-8")
            print(f"::warning::Gemini model {model} failed [{e.code}]: {err}")
            last_error = f"HTTP {e.code}: {err}"
            continue

    return f"⚠️ **Google AI Review Failed**: All attempted models failed. Last error: {last_error}"

def main():
    github_token = get_env_or_exit("GITHUB_TOKEN")
    gemini_api_key = os.environ.get("GEMINI_API_KEY")
    repository = get_env_or_exit("GITHUB_REPOSITORY")
    pr_number = get_env_or_exit("PR_NUMBER")
    
    if not gemini_api_key:
        msg = (
            "### 🤖 Google AI PR Review Agent\n\n"
            "⚠️ **Gemini API Key Missing**: Please configure the `GEMINI_API_KEY` repository secret under "
            "**Settings ➔ Secrets and variables ➔ Actions ➔ New repository secret** to enable automated AI code reviews."
        )
        print("::warning::GEMINI_API_KEY secret is not set. Skipping AI review.")
        github_api_request(
            f"https://api.github.com/repos/{repository}/issues/{pr_number}/comments",
            github_token,
            method="POST",
            data={"body": msg}
        )
        return

    # 1. Fetch PR details (Title, Body)
    _, pr_json_str = github_api_request(
        f"https://api.github.com/repos/{repository}/pulls/{pr_number}",
        github_token
    )
    pr_data = json.loads(pr_json_str)
    pr_title = pr_data.get("title", "")
    pr_body = pr_data.get("body", "")

    # 2. Fetch PR Diff
    _, pr_diff = github_api_request(
        f"https://api.github.com/repos/{repository}/pulls/{pr_number}",
        github_token,
        accept="application/vnd.github.v3.diff"
    )

    # Truncate diff if extremely large to fit token limits
    max_diff_chars = 60000
    if len(pr_diff) > max_diff_chars:
        pr_diff = pr_diff[:max_diff_chars] + "\n\n... [Diff truncated for size limit] ..."

    # 3. Read Constitution if present
    constitution = ""
    constitution_path = ".specify/memory/constitution.md"
    if os.path.exists(constitution_path):
        with open(constitution_path, "r", encoding="utf-8") as f:
            constitution = f.read()

    # 4. Construct Audit Prompt
    prompt = f"""You are the Lead Systems Architect & Principal Review Agent for Shopizy, an enterprise headless e-commerce microservices platform built on .NET 10, Clean Architecture, and Spec-Driven Development (SDD).

Perform a thorough, adversarial code review of the following Pull Request:

## PR Metadata:
- **Title**: {pr_title}
- **PR Description**:
{pr_body}

## Project Constitution & Standards:
{constitution}

## Pull Request Code Diff:
```diff
{pr_diff}
```

## Review Guidelines & Decision Rubric:

### Verdict Decision Matrix:
The Review Agent MUST follow this deterministic decision matrix:
- **`❌ CHANGES REQUESTED` (DO NOT APPROVE)**: MUST be chosen if ANY finding has severity `[Critical]` or `[Major]`:
  - **Constitution Violation**: Domain layer references external frameworks/ORMs, cross-database joins across service boundaries, or violation of Clean Architecture layer dependencies.
  - **Security Vulnerability**: Unhandled sensitive PII, hardcoded secrets, missing authorization checks, SQL injection, or leaking internal exception stack traces (violating RFC 7807).
  - **Business Invariant Risk**: Concurrency race conditions or overselling risk in inventory, missing 15-minute unpaid order expiration, or missing idempotency on payment/order submission.
  - **Test Gap / Build Breakage**: Core acceptance criteria lack automated test coverage, tests contain trivial or empty assertions, or compiler warnings exist under `--warnaserror`.
- **`⚠️ APPROVED WITH SUGGESTIONS` (APPROVE WITH NON-BLOCKING COMMENTS)**: MUST be chosen when ALL critical gates and acceptance criteria pass cleanly (zero blockers, 100% test pass rate, 0 warnings), but there are non-blocking findings with severity `[Minor]` or `[Nit]`:
  - Developer ergonomics (e.g., local Docker persistence volumes like `.WithDataVolume()`).
  - Performance improvements on non-critical paths.
  - Code clarity, documentation, or naming polish.
- **`✅ APPROVED` (FULL PRODUCTION APPROVAL)**: MUST be chosen ONLY when:
  - All acceptance criteria are fully met and verified.
  - Strict compliance with the Project Constitution and Clean Architecture.
  - Automated tests pass with 100% coverage of domain invariants and zero warnings.
  - Exactly ZERO findings ([Critical], [Major], [Minor], or [Nit]) remain.

---

## Expected Output Format:
Generate a clean, professional GitHub PR review markdown report structured as:

### Summary & Verdict
- **Verdict**: `✅ APPROVED`, `⚠️ APPROVED WITH SUGGESTIONS`, or `❌ CHANGES REQUESTED`
- **Executive Summary**: 2-3 concise sentences stating why this verdict was given, summarizing quality, architectural alignment, and deployment readiness.

### Key Strengths
- 2-4 bullet points highlighting well-implemented architecture, clean code patterns, or high-value test suites.

### Audit Findings & Improvement Suggestions
For each finding, format exactly like a staff engineer line-by-line code review:
#### 📍 `<file-path>:L<start>-L<end>` — [Severity: Critical/Major/Minor/Nit] <Finding Title>
- **Issue**: Precise description of what is wrong, risky, or sub-optimal.
- **Current Code**:
```<lang>
<exact snippet from the diff with line context>
```
- **Suggested Fix**:
```<lang>
<concrete, copy-pasteable replacement code or diff block>
```
- **Rationale**: Explain the engineering reason (e.g., prevents production probe failure, eliminates overselling race condition, satisfies Constitution Principle V).

Line Number Reference Rules:
- Calculate line numbers from the unified diff hunk headers `@@ -old,count +new,count @@` (use the `+` side corresponding to new/modified lines).
- For a multi-line range, use: `<file-path>:L<start>-L<end>` (e.g. `src/Shopizy.ServiceDefaults/Extensions.cs:L88-L98`).
- For a single line, use: `<file-path>:L<line>` (e.g. `src/Shopizy.AppHost/Program.cs:L6`).
- (If no findings exist, write: "None. All inspected lines comply with standards.")

### Verification Status & Quality Gates
- Compiler health check (`--warnaserror`).
- Automated test coverage & pass rate check.

IMPORTANT: Do NOT output a top-level header title (e.g. '## 🤖 Google AI Code Review Agent') or a sign-off footer; these are added by the automation system. Begin directly with '### Summary & Verdict'.
Keep your response constructive, professional, and directly actionable.
"""

    print("Sending code diff to Google AI (Gemini)...")
    review_markdown = call_google_ai_gemini(prompt, gemini_api_key).strip()

    # Clean up duplicate headers if model still included one
    for prefix in [
        "## 🤖 Google AI Code Review Agent",
        "# 🤖 Google AI Code Review Agent",
        "### 🤖 Google AI Code Review Agent",
        "## Google AI Code Review Agent",
        "# Google AI Code Review Agent"
    ]:
        if review_markdown.startswith(prefix):
            review_markdown = review_markdown[len(prefix):].strip()

    # Clean up duplicate footer if model included one
    for snippet in [
        "*Automated review generated by Google AI Gemini in GitHub Actions.*",
        "*Automated review generated by Google AI Gemini*",
        "*Automated review generated by Gemini in GitHub Actions.*"
    ]:
        if snippet in review_markdown:
            idx = review_markdown.rfind(snippet)
            review_markdown = review_markdown[:idx].rstrip(" -\n*")

    header = "## 🤖 Google AI Code Review Agent\n\n"
    footer = "\n\n---\n*Automated review generated by Google AI Gemini in GitHub Actions.*"
    full_comment = header + review_markdown + footer

    # 5. Post review comment to GitHub PR
    print(f"Posting review comment to PR #{pr_number}...")
    status, _ = github_api_request(
        f"https://api.github.com/repos/{repository}/issues/{pr_number}/comments",
        github_token,
        method="POST",
        data={"body": full_comment}
    )

    if status in (200, 201):
        print("::notice::Google AI review comment successfully posted to Pull Request.")
    else:
        print(f"::error::Failed to post comment. Status code: {status}")

if __name__ == "__main__":
    main()
