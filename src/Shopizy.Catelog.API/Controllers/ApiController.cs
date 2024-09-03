using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Shopizy.Catelog.API.Controllers;

public class ApiController : ControllerBase
{
    protected ActionResult Problem(IList<Error> errors)
    {
        if (errors == null || errors.Count is 0)
        {
            return Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        return Problem(errors[0]);
    }

    private ObjectResult Problem(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Problem(statusCode: statusCode, title: error.Description);
    }

    private ActionResult ValidationProblem(IList<Error> errors)
    {
        var modelStateDictonary = new ModelStateDictionary();
        foreach (Error error in errors)
        {
            modelStateDictonary.AddModelError(error.Code, error.Description);
        }
        return ValidationProblem(modelStateDictonary);
    }
}
