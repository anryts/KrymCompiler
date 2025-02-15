﻿using Common.Enums;

namespace Common.Extensions;

public static class StringExtension
{
    public static OperationType GetTypeOfOperation(this string str)
    {
        return str switch
        {
            "+" => OperationType.Add,
            "-" => OperationType.Subtract,
            "*" => OperationType.Multiply,
            "/" => OperationType.Divide,
            "==" => OperationType.Equal,
            "!=" => OperationType.NotEqual,
            ">" => OperationType.GreaterThan,
            "<" => OperationType.LessThan,
            ">=" => OperationType.GreaterThanOrEqual,
            "<=" => OperationType.LessThanOrEqual,
            _ => throw new Exception($"The {str} is not a operation"),
        };
    }
}
