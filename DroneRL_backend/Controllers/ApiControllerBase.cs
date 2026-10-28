using System;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Exceptions;

namespace DroneRL_backend.Controllers
{
    // Общий предок контроллеров: вызывает метод сервиса и превращает
    // исключения бизнес-логики в HTTP-ответы (404 / 400) с сообщением об ошибке
    public abstract class ApiControllerBase : ControllerBase
    {
        // 200 OK + результат
        protected ActionResult<T> Execute<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception e) when (IsApiError(e))
            {
                return ErrorResult(e);
            }
        }

        // 201 Created + результат и ссылка на созданный объект
        protected ActionResult<T> ExecuteCreated<T>(Func<T> action, string getActionName, Func<T, object> routeValues)
        {
            try
            {
                var result = action();
                return CreatedAtAction(getActionName, routeValues(result), result);
            }
            catch (Exception e) when (IsApiError(e))
            {
                return ErrorResult(e);
            }
        }

        // 204 No Content
        protected IActionResult ExecuteNoContent(Action action)
        {
            try
            {
                action();
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
