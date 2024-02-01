namespace Si.IdCheck.ApiClients.Cloudcheck.Constants;
public enum CloudcheckErrorCodeEnum
{
    AccessKeyMissingOrIncorrect = 101,

    RequiredFieldMissingOrEmpty = 102,

    TimestampTooOld = 103,

    NonceParameterPreviouslyUsed = 104,

    InvalidVerificationToken = 105,

    InvalidSignature = 106,

    InvalidParameterFormat = 107,

    AccessDenied = 108,

    NoResults = 110,

    QueryFailed = 111,

    QueryNotPerformed = 112,

    OperationNotPerformed = 113
}