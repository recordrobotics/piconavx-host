using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public abstract class Controller
    {
        public abstract void Subscribe();
        public abstract void Unsubscribe();
    }
}
