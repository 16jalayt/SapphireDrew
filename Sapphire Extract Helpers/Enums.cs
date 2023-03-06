using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sapphire_Extract_Helpers
{
    public static class Enums
    {
        public static string[] soundChannel = { "SS_THEME_CHAN0", "SS_MUSIC_SUPP_CHAN0", "SS_MUSIC_SUPP_CHAN1", "SS_MUSIC_SUPP_CHAN2",
            "SS_AMB_CHAN0", "SS_AMB_CHAN1", "SS_AMB_CHAN2", "SS_AMB_CHAN3", "SS_SPEC_EFFECT_CHAN0", "SS_SPEC_EFFECT_CHAN1",
            "SS_SPEC_EFFECT_CHAN2", "SS_SPEC_EFFECT_CHAN3", "SS_BS_VOICE", "SS_PLAYER_VOICE" };

        public static string[] tf = { "FALSE", "TRUE" };
        public static string[] loop = { "LOOP_INFINITE", "LOOP_ONCE" };
        public static string[] execType = { "UNKNOWN", "AE_SINGLE_EXEC" };
        public static string[] CCTEXT_TYPE = { "_NONE", "_SHORT", "_SCROLL" };
        public static string[] depFlag = { "OR_DEPENDENCY_OFF", "OR_DEPENDENCY_ON" };
        public static string[] z = { "Null", "VIEWPORT_OVERLAY1_Z" };

        public static string[] depType = { "Null", "DT_INVENTORY", "DT_EVENT", "DT_LOGIC",
            "DT_ELAPSED_GAME_TIME", "DT_ELAPSED_SCENE_TIME", "DT_ELAPSED_PLAYER_TIME" };

        public static Dictionary<int, string> ACT_Type =
              new Dictionary<int, string>(){
                   {52, "AT_OVERLAY"},
                   {145, "AT_START_SOUND"},
                   {147, "AT_SET_VOLUME"}};
    }
}