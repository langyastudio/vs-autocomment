// Guids.cs
// MUST match guids.h
using System;

namespace HKFY.AutoComment2013
{
    static class GuidList
    {
        public const string guidAutoComment2013PkgString = "eca609f2-905d-4007-8721-c7d02fd49ff1";
        public const string guidAutoComment2013CmdSetString = "6c3ca1a7-2435-4e52-aa5b-79bbfd2bfc72";

        public static readonly Guid guidAutoComment2013CmdSet = new Guid(guidAutoComment2013CmdSetString);
    };
}