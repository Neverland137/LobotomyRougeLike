using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public class MemeModel
    {
        public MemeInfo metaInfo;
        public int instanceId;
        public MemeScriptBase script = new MemeScriptBase();
    }
}
