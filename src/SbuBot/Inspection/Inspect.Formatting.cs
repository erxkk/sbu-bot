using System.Text;

namespace SbuBot.Inspection
{
    public partial class Inspect
    {
        public const int MAX_EMBED_CODE_BLOCK_WIDTH = 60;

        public static void InlineShortExpansions(StringBuilder builder)
        {
            // TODO

            // maybe append like this:
            // ActualProp: {
            //   SubProp: "too long expansions"
            //   + "continuation as concatenation"
            // }

            // EnumerableProp: [
            //   A,
            //   B,
            // ]
            // to
            // EnumerableProp: [ A, B ]
        }
    }
}