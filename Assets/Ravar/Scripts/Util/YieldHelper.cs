using UnityEngine;

namespace Itsdits.Ravar.Util
{
    /// <summary>
    /// Static helper class that pools yields to avoid garbage collection being triggered.
    /// </summary>
    public static class YieldHelper
    {
        public static WaitForSeconds TypingTime = new WaitForSeconds(0.05f);
        public static WaitForSeconds FifthSecond = new WaitForSeconds(0.2f);
        public static WaitForSeconds HalfSecond = new WaitForSeconds(0.5f);
        public static WaitForSeconds OneSecond = new WaitForSeconds(1f);
        public static WaitForSeconds TwoSeconds = new WaitForSeconds(2f);
        public static WaitForSeconds TwoAndChangeSeconds = new WaitForSeconds(2.1f);
        public static WaitForSeconds ThreeSeconds = new WaitForSeconds(3f);
        public static WaitForSeconds FourSeconds = new WaitForSeconds(4f);
        public static WaitForEndOfFrame EndOfFrame = new WaitForEndOfFrame();
    }
}