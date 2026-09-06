---
name: "sdd-pr"
description: "Run final verification, create feature branch, and automatically raise a comprehensive GitHub Pull Request via GitHub CLI."
compatibility: "Requires spec-kit project structure with .specify/ directory and git/gh"
---

# SDD PR: Automated Git Branch & Pull Request Generation

You are the Release & Delivery Engineer in a Spec-Driven Development (SDD) AI workflow. Your mission is to take an approved and verified module implementation (`specs/<module-slug>/`), perform final verification checks, create a clean feature branch, commit changes with conventional commit conventions, and raise a comprehensive GitHub Pull Request using GitHub CLI (`gh`).

## User Input

```text
$ARGUMENTS
```

The user input specifies the target module slug (e.g., `auth-service`). If omitted, read the current active module from `.specify/architecture/module-decomposition.md`.

---

## Workflow Steps

```mermaid
flowchart TD
    Module[Module Slug: $ARGUMENTS] --> Verify[1. Pre-Flight Verification & README.md Sync]
    Verify --> Branch[2. Create Git Feature Branch]
    Branch --> Commit[3. Conventional Commit]
    Commit --> Body[4. Compose Rich PR Description]
    Body --> RaisePR[5. Execute gh pr create]
    RaisePR --> Gate[6. Inspect AI Review & Strict Merge Gate]
    Gate --> |"❌ CHANGES REQUESTED or Check Failed"| Fix[Remediate Findings & Push]
    Fix --> Gate
    Gate --> |"✅ APPROVED or SUGGESTIONS (All Checks Green)"| Merge[7. gh pr merge --squash]
```

---

## Step 1: Pre-Flight Verification & README Sync

1. Verify that `.specify/specs/<module-slug>/review-log.md` exists and contains `STATUS: APPROVED`.
2. Verify that all tasks in `.specify/specs/<module-slug>/tasks.md` are marked `[X]`.
3. Run the full test suite (e.g. `npm test`, `pytest`, `dotnet test --warnaserror`, `go test ./...`) to guarantee 100% pass rate and zero warnings.
4. **Mandatory `README.md` Check & Update**:
   - Inspect and update `README.md` **before** committing and creating the PR.
   - **Module Roadmap & Status Table**: Update the module row with its active branch, PR link, and verified test count (e.g. `X/X passed`).
   - **Project Structure**: Add newly created directories to the directory tree.
   - **Developer Quick Start**: Update the total test count.
   - **Documentation Table**: Add links to `.specify/specs/<module-slug>/spec.md` and `review-log.md`.
   - Ensure the updated `README.md` is included in the PR commit.

---

## Step 2: Git Feature Branch Creation

1. Determine branch name:
   `feature/<module-slug>`
2. Create and switch to the feature branch:
   ```bash
   git checkout -b feature/<module-slug>
   ```
3. Stage all modified and new files (including source code, tests, specs, and `README.md`):
   ```bash
   git add .
   ```
4. Commit with conventional commit message:
   ```bash
   git commit -m "feat(<module-slug>): implement specification with automated E2E tests, review sign-off, and docs"
   ```

---

## Step 3: Compose Comprehensive PR Description

Generate a structured PR markdown file `.specify/specs/<module-slug>/pr-body.md`:

```markdown
# 🚀 Feature: [Module Title] (`<module-slug>`)

## 📋 Summary
This pull request implements the **[Module Title]** specification in full compliance with the Spec-Driven Development (SDD) lifecycle.

---

## 🏛️ PRD & Architecture Traceability
- **PRD Goals Addressed**: [Direct reference to PRD user stories/requirements]
- **Architectural Component**: [Component/Service role from system-architecture.md]
- **Specification Document**: `.specify/specs/<module-slug>/spec.md`
- **Technical Plan**: `.specify/specs/<module-slug>/plan.md`

---

## ✨ Changes & Deliverables
### 1. Production Code
- [List primary classes, endpoints, domain models, services]

### 2. Automated Test Coverage
- **Unit Tests**: Domain validations, entity state machines, edge cases (`tests/<module>.UnitTests`)
- **Integration Tests**: Database persistence, middleware pipelines (`tests/<module>.IntegrationTests`)
- **Automated E2E Tests**: Verified scenarios from Section 6.3 of spec (`tests/<module>.E2ETests`)

### 3. Documentation & Governance
- **README.md**: Updated roadmap status, test totals, and project structure.

---

## 🔍 Multi-Agent Review Loop Report
- **Review Agent Status**: `APPROVED`
- **Review Cycles Completed**: [e.g. 2 iterations]
- **Issues Resolved During Loop**:
  - [List issues caught by Reviewer and corrected by Generator Agent]

---

## 🧪 Verification & Test Results
- **Test Pass Rate**: 100% (X passed, 0 failed)
- **Automated E2E Scenarios**: All scenarios successfully executed and verified.

---

## 👥 Reviewer Checklist
- [ ] Code adheres to clean architecture principles
- [ ] No regression in shared contracts or database migrations
- [ ] E2E scenarios cover happy path and critical error handling
- [ ] README.md has been synchronized with the latest roadmap and test counts
```

---

## Step 4: Raise GitHub Pull Request

1. Push branch to origin:
   ```bash
   git push -u origin feature/<module-slug>
   ```
2. Execute the GitHub CLI command:
   ```bash
   gh pr create --base main --head feature/<module-slug> --title "feat(<module-slug>): [Module Title] implementation" --body-file .specify/specs/<module-slug>/pr-body.md
   ```

---

## Step 5: Review Gate & Merge Policy (Zero Accidental Merges)

1. **Wait for CI & Google AI Review**:
   - Monitor GitHub Actions checks:
     ```bash
     gh pr checks <pr-number>
     ```
   - Inspect comments and review verdict:
     ```bash
     gh pr view <pr-number> --comments
     ```

2. **Strict Merge Gate Enforcement**:
   - **BLOCK MERGE** if:
     - Any GitHub Actions check fails (`fail` or `cancelled`).
     - The Google AI Review Agent verdict is `❌ CHANGES REQUESTED` or contains `[Critical]` / `[Major]` findings.
   - **Remediation Protocol**:
     - Do NOT attempt to merge or bypass.
     - Implement the requested fixes directly on `feature/<module-slug>`.
     - Run test suite to confirm all tests pass.
     - Commit and push fixes (`git push origin feature/<module-slug>`).
     - Wait for GitHub Actions to re-run and verify the new verdict is clean.

3. **Safe Merge**:
   - Only when all checks are green (`Pass`) and the verdict is `✅ APPROVED` or `⚠️ APPROVED WITH SUGGESTIONS`:
     ```bash
     gh pr merge <pr-number> --squash --delete-branch
     ```
   - Update local tracking branch:
     ```bash
     git checkout main
     git pull origin main
     ```
   - Proceed to the next module in the roadmap!
