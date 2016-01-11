﻿using System.ServiceModel.Channels;

namespace Insight.WS.Log.Entity
{
    /// <summary>
    /// WebContentTypeMapper
    /// </summary>
    public class TypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            return WebContentFormat.Json;
        }

    }

}
