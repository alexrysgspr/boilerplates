﻿namespace Si.IdCheck.ApiClients.Cloudcheck.Models.Requests;

public class PeidLookupRequest : CloudcheckPostRequestBase, IParameterBuilder, IPostRequestBuilder
{
    public int Peid { get; set; }

    public SortedDictionary<string, string> BuildParameter(SortedDictionary<string, string> baseDictionary)
    {
        baseDictionary.Add(nameof(Peid).ToLower(), Peid.ToString());
        return baseDictionary;
    }

    public CloudcheckPostRequestBase BuildPostRequest(string key, string nonce, string signature, string timestamp)
    {
        return new PeidLookupRequest
        {
            Peid = Peid,
            Key = key,
            Nonce = nonce,
            Signature = signature,
            TimeStamp = timestamp
        };
    }
}

