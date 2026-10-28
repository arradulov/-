using System;

namespace DroneRL_backend.Exceptions
{
    // Нарушено бизнес-правило (например, нельзя активировать дрон с низким зарядом) → 400
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
    }

    // Объект с таким id не найден → 404
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
