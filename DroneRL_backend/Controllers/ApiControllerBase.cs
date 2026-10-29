using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Exceptions;

namespace DroneRL_backend.Controllers
{

    public abstract class ApiControllerBase : ControllerBase
    {

        protected async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception e) when (IsApiError(e))
            {
                return ErrorResult(e);
            }
        }


        protected async Task<ActionResult<T>> ExecuteCreatedAsync<T>(Func<Task<T>> action, string getActionName, Func<T, object> routeValues)
        {
            try
            {
                var result = await action();
                return CreatedAtAction(getActionName, routeValues(result), result);
            }
            catch (Exception e) when (IsApiError(e))
            {
                return ErrorResult(e);
            }
        }


        protected async Task<IActionResult> ExecuteNoContentAsync(Func<Task> action)
        {
            try
            {
                await action();
                return NoContent();
            }
            catch (Exception e) when (IsApiError(e))
            {
                return ErrorResult(e);
            }
        }

        private static bool IsApiError(Exception e) => e is NotFoundException || e is BusinessRuleException;

        private ActionResult ErrorResult(Exception e)
        {
            if (e is NotFoundException)
                return NotFound(new { message = e.Message });
            return BadRequest(new { message = e.Message });
        }
    }
}
