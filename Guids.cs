// Guids.cs
// MUST match guids.h
using System;

namespace ForceLineFeedCode
{
    static class GuidList
    {
        public const string guidForceEncPkgString = "fbf9519c-d95e-4353-8195-dfcfbd985b9e";
        public const string guidForceEncCmdSetString = "438a8c81-3037-4f3f-8989-a08b3a058574";

        public static readonly Guid guidForceEncCmdSet = new Guid(guidForceEncCmdSetString);
    };
}