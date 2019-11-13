using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class RequestStringPackage : Package
    {
        public enum EOp { Request, Response }
        public EOp Op { get; set; }
        public string RequestCache { get; set; } = "";
        public string Content { get; set; } = "";


        public RequestStringPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.REQUEST_STRING;

            Args = new PackageArgument[]
            {
                new PackageArgument_Byte(),
                new PackageArgument_String(),
                new PackageArgument_String(),
            };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Byte).Value = (byte)Op;
            (Args[1] as PackageArgument_String).Value = RequestCache;
            (Args[2] as PackageArgument_String).Value = Content;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);

            Op = (EOp)(Args[0] as PackageArgument_Byte).Value;
            RequestCache = (Args[1] as PackageArgument_String).Value;
            Content = (Args[2] as PackageArgument_String).Value;

            return result;
        }
    }
}
