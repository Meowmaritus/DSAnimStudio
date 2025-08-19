namespace DSAnimStudio
{
    public partial class TaeSearch
    {
        public class Result
        {
            public DSAProj.AnimCategory Category;
            public DSAProj.Animation Animation;
            public DSAProj.Action Action;

            public static Result FromCheck(Check check)
            {
                var res = new Result();
                res.Category = check.Category;
                res.Animation = check.Anim;
                res.Action = check.Act;
                return res;
            }
        }
    }
}