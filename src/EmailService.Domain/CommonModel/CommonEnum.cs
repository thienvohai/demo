namespace EmailService.Domain;

public enum ErrorCode
{
    Unknow = 0
}

public enum EmailStatus
{
    New = 0,
    Sent = 1,
    Failed = 2,
    Filtered = 3
}