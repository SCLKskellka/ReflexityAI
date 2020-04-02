﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plugins.xNodeUtilityAi.Framework;
using Plugins.xNodeUtilityAi.Utils;
using UnityEngine;
using XNode;

namespace Plugins.xNodeUtilityAi.MainNodes {
    public class DataReaderNode : DataNode, IContextual {

        public AbstractAIComponent Context { get; set; }

        private readonly List<SerializableMemberInfo> SerializableMemberInfos = new List<SerializableMemberInfo>();

        private void OnValidate() {
            List<MemberInfo> memberInfos = GetMemberInfos();
            // Add new ports
            foreach (MemberInfo memberInfo in memberInfos) {
                if (memberInfos.Any(info => info.Name == memberInfo.Name)) continue;
                AddDynamicOutput(memberInfo.FieldType(), ConnectionType.Multiple, TypeConstraint.None, memberInfo.Name);
                SerializableMemberInfos.Add(memberInfo.ToSerializableMemberInfo());
            }
            // Remove old ports
            foreach (SerializableMemberInfo serializableMemberInfo in SerializableMemberInfos) {
                if (memberInfos.All(info => info.Name != serializableMemberInfo.FieldName)) continue;
                RemoveDynamicPort(serializableMemberInfo.FieldName);
                SerializableMemberInfos.Remove(serializableMemberInfo);
            }
        }

        public override object GetValue(NodePort port) {
            return Application.isPlaying
                ? GetFullValue(port.fieldName)
                : GetReflectedValue(port.fieldName);
        }

        public override object GetReflectedValue(string portName) {
            SerializableMemberInfo firstOrDefault = SerializableMemberInfos.FirstOrDefault(info => info.FieldName == portName);
            if (firstOrDefault == null) throw new Exception("No reflected data found for " + portName);
            MemberInfo memberInfo = firstOrDefault.ToMemberInfo();
            return new ReflectionData(memberInfo.Name, memberInfo.FieldType(), null);
        }

        public override object GetFullValue(string portName) {
            SerializableMemberInfo firstOrDefault = SerializableMemberInfos.FirstOrDefault(info => info.FieldName == portName);
            if (firstOrDefault == null) throw new Exception("No reflected data found for " + portName);
            MemberInfo memberInfo = firstOrDefault.ToMemberInfo();
            return new ReflectionData(memberInfo.Name, memberInfo.FieldType(), memberInfo.GetValue(Context));
        }
        
        private List<MemberInfo> GetMemberInfos() {
            if (graph is AIBrainGraph brainGraph && brainGraph.ContextType != null) {
                return brainGraph.ContextType.Type.GetMemberInfos().ToList();
            }
            throw new Exception("No brain graph context type found, please select one");
        }
        
    }
}

