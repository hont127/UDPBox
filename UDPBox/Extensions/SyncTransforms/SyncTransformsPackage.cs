using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class SyncTransformsPackage : Package
    {
        public enum EOperate { Push, Fetch, FetchAll, RestoreList }
        public EOperate Op { get; set; }
        public List<SyncTransformData> TransformList { get; set; }
        public override bool EnabledCompress => true;


        public SyncTransformsPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.SYNC_TRANSFORMS;

            TransformList = new List<SyncTransformData>(12);

            Args = new PackageArgument[]
            {
                new PackageArgument_Int(),
                new PackageArgument_TransformList(),
            };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Int).Value = (int)Op;

            var transformList = (Args[1] as PackageArgument_TransformList).Value;

            transformList.Clear();
            for (int i = 0, iMax = TransformList.Count; i < iMax; i++)
            {
                var self_item = TransformList[i];

                var item = new PackageArgument_TransformList.TransformInfo();
                item.ID = self_item.NetworkID;
                item.Pos_X = self_item.Pos_X;
                item.Pos_Y = self_item.Pos_Y;
                item.Pos_Z = self_item.Pos_Z;
                item.Euler_X = self_item.Euler_X;
                item.Euler_Y = self_item.Euler_Y;
                item.Euler_Z = self_item.Euler_Z;
                item.SCALE_X = self_item.SCALE_X;
                item.SCALE_Y = self_item.SCALE_Y;
                item.SCALE_Z = self_item.SCALE_Z;
                transformList.Add(item);
            }

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return result;

            Op = (EOperate)(Args[0] as PackageArgument_Int).Value;

            var arg_transformList = (Args[1] as PackageArgument_TransformList).Value;

            TransformList.Clear();
            for (int i = 0; i < arg_transformList.Count; i++)
            {
                var arg_transformInfo = arg_transformList[i];
                var item = new SyncTransformData();
                item.NetworkID = arg_transformInfo.ID;
                item.Pos_X = arg_transformInfo.Pos_X;
                item.Pos_Y = arg_transformInfo.Pos_Y;
                item.Pos_Z = arg_transformInfo.Pos_Z;
                item.Euler_X = arg_transformInfo.Euler_X;
                item.Euler_Y = arg_transformInfo.Euler_Y;
                item.Euler_Z = arg_transformInfo.Euler_Z;
                item.SCALE_X = arg_transformInfo.SCALE_X;
                item.SCALE_Y = arg_transformInfo.SCALE_Y;
                item.SCALE_Z = arg_transformInfo.SCALE_Z;
                TransformList.Add(item);
            }

            return result;
        }
    }
}
