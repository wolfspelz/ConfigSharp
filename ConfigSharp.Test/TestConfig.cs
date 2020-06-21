using System;

namespace ConfigSharp.Test
{
    public class TestConfig : Container
    {
        public int IntMember;
        public string StringMember;
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }
        public DateTime DateTimeMember;
        public DateTime DateTimeProperty { get; set; }
        public string ExecuteCodeWithReferenceResult { get; set; }
    }
}
