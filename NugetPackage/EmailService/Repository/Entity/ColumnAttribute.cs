﻿namespace EmailService;

public class ColumnAttribute : Attribute
{
    public ColumnAttribute(string tableName)
    {
        Name = tableName;
    }

    public string Name { get; set; }
}
