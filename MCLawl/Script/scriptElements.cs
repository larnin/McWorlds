using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MCWorlds
{
    public class variable
    {

        public List<int> vars;
        public string name;

        public variable(string varName) { name = varName; }
    }

#region == Classes elements ==
    /*valeurs type
     * 0 : null     1 : if      2 : else    3 : endif
     * 4 : boucle   5 : endboucle           6 : changebloc
     * 7 : move     8 : variable            9 : commande
     * 10: message  11: wait    12: stop
     */
    public abstract class baseElements
    {
        script scriptBase = null;
        protected int type = 0;

        public abstract void exec();

        public int getType() { return type; }
    }

    public class elementIf : baseElements
    {
        public int lineElse;
        public int lineEndif;

        public variable var1, var2;
        public string svar1, svar2;
        public bool[] signe = { false, false, false };

        public elementIf(string nameVar1, string nameVar2, string nameSigne)
        {
            type = 1;
            lineElse = 0;
            lineEndif = 0;
        }

        public override void exec()
        {

        }

    }

    public class elementElse : baseElements
    {
        public int lineEndif;

        public elementElse()
        {
            type = 2;
            lineEndif = 0;
        }

        public override void exec()
        {

        }
    }

    public class elementEndIf : baseElements
    {

        public elementEndIf()
        {
            type = 3;
        }

        public override void exec()
        {

        }
    }

    public class elementBoucle : baseElements
    {
        public int lineEndBoucle;

        public variable var1, var2;
        public string svar1, svar2;
        public bool[] signe = { false, false, false };

        public elementBoucle(string nameVar1, string nameVar2, string nameSigne)
        {
            type = 4;
            lineEndBoucle = 0;
        }

        public override void exec()
        {
            
        }
    }

    public class elementEndBoucle : baseElements
    {
        public elementEndBoucle()
        {
            type = 5;
        }

        public override void exec()
        {

        }
    }

    public class elementChangebloc : baseElements
    {
        public override void exec()
        {

        }
    }

    public class elementMove : baseElements
    {
        public override void exec()
        {

        }
    }

    public class elementVar : baseElements
    {
        public override void exec()
        {

        }
    }

    public class elementCommande : baseElements
    {
        public override void exec()
        {

        }
    }

    public class elementMessage : baseElements
    {
        public override void exec()
        {

        }
    }

    public class elementWait : baseElements
    {
        public override void exec()
        {

        }
    }

    public class elementStop : baseElements
    {
        public override void exec()
        {

        }
    }    
#endregion
}
