// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: services.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Gauge.Messages {

  /// <summary>Holder for reflection information generated from services.proto</summary>
  public static partial class ServicesReflection {

    #region Descriptor
    /// <summary>File descriptor for services.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ServicesReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg5zZXJ2aWNlcy5wcm90bxIOZ2F1Z2UubWVzc2FnZXMaDm1lc3NhZ2VzLnBy",
            "b3RvMsMSCgZSdW5uZXISWQoMVmFsaWRhdGVTdGVwEiMuZ2F1Z2UubWVzc2Fn",
            "ZXMuU3RlcFZhbGlkYXRlUmVxdWVzdBokLmdhdWdlLm1lc3NhZ2VzLlN0ZXBW",
            "YWxpZGF0ZVJlc3BvbnNlEm4KGEluaXRpYWxpemVTdWl0ZURhdGFTdG9yZRIp",
            "LmdhdWdlLm1lc3NhZ2VzLlN1aXRlRGF0YVN0b3JlSW5pdFJlcXVlc3QaJy5n",
            "YXVnZS5tZXNzYWdlcy5FeGVjdXRpb25TdGF0dXNSZXNwb25zZRJjCg5TdGFy",
            "dEV4ZWN1dGlvbhIoLmdhdWdlLm1lc3NhZ2VzLkV4ZWN1dGlvblN0YXJ0aW5n",
            "UmVxdWVzdBonLmdhdWdlLm1lc3NhZ2VzLkV4ZWN1dGlvblN0YXR1c1Jlc3Bv",
            "bnNlEmwKF0luaXRpYWxpemVTcGVjRGF0YVN0b3JlEiguZ2F1Z2UubWVzc2Fn",
            "ZXMuU3BlY0RhdGFTdG9yZUluaXRSZXF1ZXN0GicuZ2F1Z2UubWVzc2FnZXMu",
            "RXhlY3V0aW9uU3RhdHVzUmVzcG9uc2USawoSU3RhcnRTcGVjRXhlY3V0aW9u",
            "EiwuZ2F1Z2UubWVzc2FnZXMuU3BlY0V4ZWN1dGlvblN0YXJ0aW5nUmVxdWVz",
            "dBonLmdhdWdlLm1lc3NhZ2VzLkV4ZWN1dGlvblN0YXR1c1Jlc3BvbnNlEnQK",
            "G0luaXRpYWxpemVTY2VuYXJpb0RhdGFTdG9yZRIsLmdhdWdlLm1lc3NhZ2Vz",
            "LlNjZW5hcmlvRGF0YVN0b3JlSW5pdFJlcXVlc3QaJy5nYXVnZS5tZXNzYWdl",
            "cy5FeGVjdXRpb25TdGF0dXNSZXNwb25zZRJzChZTdGFydFNjZW5hcmlvRXhl",
            "Y3V0aW9uEjAuZ2F1Z2UubWVzc2FnZXMuU2NlbmFyaW9FeGVjdXRpb25TdGFy",
            "dGluZ1JlcXVlc3QaJy5nYXVnZS5tZXNzYWdlcy5FeGVjdXRpb25TdGF0dXNS",
            "ZXNwb25zZRJrChJTdGFydFN0ZXBFeGVjdXRpb24SLC5nYXVnZS5tZXNzYWdl",
            "cy5TdGVwRXhlY3V0aW9uU3RhcnRpbmdSZXF1ZXN0GicuZ2F1Z2UubWVzc2Fn",
            "ZXMuRXhlY3V0aW9uU3RhdHVzUmVzcG9uc2USWgoLRXhlY3V0ZVN0ZXASIi5n",
            "YXVnZS5tZXNzYWdlcy5FeGVjdXRlU3RlcFJlcXVlc3QaJy5nYXVnZS5tZXNz",
            "YWdlcy5FeGVjdXRpb25TdGF0dXNSZXNwb25zZRJqChNGaW5pc2hTdGVwRXhl",
            "Y3V0aW9uEiouZ2F1Z2UubWVzc2FnZXMuU3RlcEV4ZWN1dGlvbkVuZGluZ1Jl",
            "cXVlc3QaJy5nYXVnZS5tZXNzYWdlcy5FeGVjdXRpb25TdGF0dXNSZXNwb25z",
            "ZRJyChdGaW5pc2hTY2VuYXJpb0V4ZWN1dGlvbhIuLmdhdWdlLm1lc3NhZ2Vz",
            "LlNjZW5hcmlvRXhlY3V0aW9uRW5kaW5nUmVxdWVzdBonLmdhdWdlLm1lc3Nh",
            "Z2VzLkV4ZWN1dGlvblN0YXR1c1Jlc3BvbnNlEmoKE0ZpbmlzaFNwZWNFeGVj",
            "dXRpb24SKi5nYXVnZS5tZXNzYWdlcy5TcGVjRXhlY3V0aW9uRW5kaW5nUmVx",
            "dWVzdBonLmdhdWdlLm1lc3NhZ2VzLkV4ZWN1dGlvblN0YXR1c1Jlc3BvbnNl",
            "EmIKD0ZpbmlzaEV4ZWN1dGlvbhImLmdhdWdlLm1lc3NhZ2VzLkV4ZWN1dGlv",
            "bkVuZGluZ1JlcXVlc3QaJy5nYXVnZS5tZXNzYWdlcy5FeGVjdXRpb25TdGF0",
            "dXNSZXNwb25zZRJECglDYWNoZUZpbGUSIC5nYXVnZS5tZXNzYWdlcy5DYWNo",
            "ZUZpbGVSZXF1ZXN0GhUuZ2F1Z2UubWVzc2FnZXMuRW1wdHkSUAoLR2V0U3Rl",
            "cE5hbWUSHy5nYXVnZS5tZXNzYWdlcy5TdGVwTmFtZVJlcXVlc3QaIC5nYXVn",
            "ZS5tZXNzYWdlcy5TdGVwTmFtZVJlc3BvbnNlEl8KD0dldEdsb2JQYXR0ZXJu",
            "cxIVLmdhdWdlLm1lc3NhZ2VzLkVtcHR5GjUuZ2F1Z2UubWVzc2FnZXMuSW1w",
            "bGVtZW50YXRpb25GaWxlR2xvYlBhdHRlcm5SZXNwb25zZRJTCgxHZXRTdGVw",
            "TmFtZXMSIC5nYXVnZS5tZXNzYWdlcy5TdGVwTmFtZXNSZXF1ZXN0GiEuZ2F1",
            "Z2UubWVzc2FnZXMuU3RlcE5hbWVzUmVzcG9uc2USXwoQR2V0U3RlcFBvc2l0",
            "aW9ucxIkLmdhdWdlLm1lc3NhZ2VzLlN0ZXBQb3NpdGlvbnNSZXF1ZXN0GiUu",
            "Z2F1Z2UubWVzc2FnZXMuU3RlcFBvc2l0aW9uc1Jlc3BvbnNlEl8KFkdldElt",
            "cGxlbWVudGF0aW9uRmlsZXMSFS5nYXVnZS5tZXNzYWdlcy5FbXB0eRouLmdh",
            "dWdlLm1lc3NhZ2VzLkltcGxlbWVudGF0aW9uRmlsZUxpc3RSZXNwb25zZRJY",
            "Cg1JbXBsZW1lbnRTdHViEi0uZ2F1Z2UubWVzc2FnZXMuU3R1YkltcGxlbWVu",
            "dGF0aW9uQ29kZVJlcXVlc3QaGC5nYXVnZS5tZXNzYWdlcy5GaWxlRGlmZhJN",
            "CghSZWZhY3RvchIfLmdhdWdlLm1lc3NhZ2VzLlJlZmFjdG9yUmVxdWVzdBog",
            "LmdhdWdlLm1lc3NhZ2VzLlJlZmFjdG9yUmVzcG9uc2USQQoES2lsbBIiLmdh",
            "dWdlLm1lc3NhZ2VzLktpbGxQcm9jZXNzUmVxdWVzdBoVLmdhdWdlLm1lc3Nh",
            "Z2VzLkVtcHR5EmgKHk5vdGlmeUNvbmNlcHRFeGVjdXRpb25TdGFydGluZxIv",
            "LmdhdWdlLm1lc3NhZ2VzLkNvbmNlcHRFeGVjdXRpb25TdGFydGluZ1JlcXVl",
            "c3QaFS5nYXVnZS5tZXNzYWdlcy5FbXB0eRJkChxOb3RpZnlDb25jZXB0RXhl",
            "Y3V0aW9uRW5kaW5nEi0uZ2F1Z2UubWVzc2FnZXMuQ29uY2VwdEV4ZWN1dGlv",
            "bkVuZGluZ1JlcXVlc3QaFS5nYXVnZS5tZXNzYWdlcy5FbXB0eTL/CAoIUmVw",
            "b3J0ZXISWgoXTm90aWZ5RXhlY3V0aW9uU3RhcnRpbmcSKC5nYXVnZS5tZXNz",
            "YWdlcy5FeGVjdXRpb25TdGFydGluZ1JlcXVlc3QaFS5nYXVnZS5tZXNzYWdl",
            "cy5FbXB0eRJiChtOb3RpZnlTcGVjRXhlY3V0aW9uU3RhcnRpbmcSLC5nYXVn",
            "ZS5tZXNzYWdlcy5TcGVjRXhlY3V0aW9uU3RhcnRpbmdSZXF1ZXN0GhUuZ2F1",
            "Z2UubWVzc2FnZXMuRW1wdHkSagofTm90aWZ5U2NlbmFyaW9FeGVjdXRpb25T",
            "dGFydGluZxIwLmdhdWdlLm1lc3NhZ2VzLlNjZW5hcmlvRXhlY3V0aW9uU3Rh",
            "cnRpbmdSZXF1ZXN0GhUuZ2F1Z2UubWVzc2FnZXMuRW1wdHkSaAoeTm90aWZ5",
            "Q29uY2VwdEV4ZWN1dGlvblN0YXJ0aW5nEi8uZ2F1Z2UubWVzc2FnZXMuQ29u",
            "Y2VwdEV4ZWN1dGlvblN0YXJ0aW5nUmVxdWVzdBoVLmdhdWdlLm1lc3NhZ2Vz",
            "LkVtcHR5EmQKHE5vdGlmeUNvbmNlcHRFeGVjdXRpb25FbmRpbmcSLS5nYXVn",
            "ZS5tZXNzYWdlcy5Db25jZXB0RXhlY3V0aW9uRW5kaW5nUmVxdWVzdBoVLmdh",
            "dWdlLm1lc3NhZ2VzLkVtcHR5EmIKG05vdGlmeVN0ZXBFeGVjdXRpb25TdGFy",
            "dGluZxIsLmdhdWdlLm1lc3NhZ2VzLlN0ZXBFeGVjdXRpb25TdGFydGluZ1Jl",
            "cXVlc3QaFS5nYXVnZS5tZXNzYWdlcy5FbXB0eRJeChlOb3RpZnlTdGVwRXhl",
            "Y3V0aW9uRW5kaW5nEiouZ2F1Z2UubWVzc2FnZXMuU3RlcEV4ZWN1dGlvbkVu",
            "ZGluZ1JlcXVlc3QaFS5nYXVnZS5tZXNzYWdlcy5FbXB0eRJmCh1Ob3RpZnlT",
            "Y2VuYXJpb0V4ZWN1dGlvbkVuZGluZxIuLmdhdWdlLm1lc3NhZ2VzLlNjZW5h",
            "cmlvRXhlY3V0aW9uRW5kaW5nUmVxdWVzdBoVLmdhdWdlLm1lc3NhZ2VzLkVt",
            "cHR5El4KGU5vdGlmeVNwZWNFeGVjdXRpb25FbmRpbmcSKi5nYXVnZS5tZXNz",
            "YWdlcy5TcGVjRXhlY3V0aW9uRW5kaW5nUmVxdWVzdBoVLmdhdWdlLm1lc3Nh",
            "Z2VzLkVtcHR5ElYKFU5vdGlmeUV4ZWN1dGlvbkVuZGluZxImLmdhdWdlLm1l",
            "c3NhZ2VzLkV4ZWN1dGlvbkVuZGluZ1JlcXVlc3QaFS5nYXVnZS5tZXNzYWdl",
            "cy5FbXB0eRJQChFOb3RpZnlTdWl0ZVJlc3VsdBIkLmdhdWdlLm1lc3NhZ2Vz",
            "LlN1aXRlRXhlY3V0aW9uUmVzdWx0GhUuZ2F1Z2UubWVzc2FnZXMuRW1wdHkS",
            "QQoES2lsbBIiLmdhdWdlLm1lc3NhZ2VzLktpbGxQcm9jZXNzUmVxdWVzdBoV",
            "LmdhdWdlLm1lc3NhZ2VzLkVtcHR5MpMBCgpEb2N1bWVudGVyEkIKDEdlbmVy",
            "YXRlRG9jcxIbLmdhdWdlLm1lc3NhZ2VzLlNwZWNEZXRhaWxzGhUuZ2F1Z2Uu",
            "bWVzc2FnZXMuRW1wdHkSQQoES2lsbBIiLmdhdWdlLm1lc3NhZ2VzLktpbGxQ",
            "cm9jZXNzUmVxdWVzdBoVLmdhdWdlLm1lc3NhZ2VzLkVtcHR5QlwKFmNvbS50",
            "aG91Z2h0d29ya3MuZ2F1Z2VaMWdpdGh1Yi5jb20vZ2V0Z2F1Z2UvZ2F1Z2Ut",
            "cHJvdG8vZ28vZ2F1Z2VfbWVzc2FnZXOqAg5HYXVnZS5NZXNzYWdlc2IGcHJv",
            "dG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Gauge.Messages.MessagesReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, null));
    }
    #endregion

  }
}

#endregion Designer generated code
