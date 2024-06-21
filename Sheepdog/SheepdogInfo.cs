using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Sheepdog
{
    public class SheepdogInfo : GH_AssemblyInfo
    {
        public override string Name => "Sheepdog";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Sheepdog adds Fences to help keep your canvas more organised";

        public override Guid Id => new Guid("e813fe18-2c1d-47cd-a9db-dac984dd655c");

        //Return a string identifying you or your company.
        public override string AuthorName => "Mule Studio";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "hello@mule.studio";
    }
}