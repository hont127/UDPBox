using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxPackage;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class RequestStringHandler : HandlerBase
    {
        RequestStringPackage mTemplate;
        Func<string, string> mOnProcessRequest;
        Action<string, string> mOnProcessResponse;


        public RequestStringHandler(byte[] packageHeadBytes, Func<string, string> onProcessRequest, Action<string, string> onProcessResponse)
        {
            mOnProcessRequest = onProcessRequest;
            mOnProcessResponse = onProcessResponse;
            mTemplate = new RequestStringPackage(packageHeadBytes);
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.REQUEST_STRING };
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            mTemplate.Deserialize(packageBytes);

            switch (mTemplate.Op)
            {
                case RequestStringPackage.EOp.Request:

                    var request = mTemplate.Content;
                    UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                    {
                        var respText = mOnProcessRequest(request);
                        mTemplate.RequestCache = request;
                        mTemplate.Content = respText;
                        mTemplate.Op = RequestStringPackage.EOp.Response;
                        udpBox.SendMessage(mTemplate.Serialize(), ipEndPoint);
                    });

                    break;
                case RequestStringPackage.EOp.Response:

                    var content = mTemplate.Content;
                    UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                    {
                        mOnProcessResponse(mTemplate.RequestCache, mTemplate.Content);
                    });

                    break;
                default:
                    break;
            }
        }
    }
}
