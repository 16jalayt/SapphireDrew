using System.Collections.Generic;

namespace Sapphire_Extract_Helpers
{
    public static class Enums
    {
        //TODO: turn all into dictionary?
        public static string[] soundChannel = { "SS_THEME_CHAN0", "SS_MUSIC_SUPP_CHAN0", "SS_MUSIC_SUPP_CHAN1", "SS_MUSIC_SUPP_CHAN2",
            "SS_AMB_CHAN0", "SS_AMB_CHAN1", "SS_AMB_CHAN2", "SS_AMB_CHAN3", "SS_SPEC_EFFECT_CHAN0", "SS_SPEC_EFFECT_CHAN1",
            "SS_SPEC_EFFECT_CHAN2", "SS_SPEC_EFFECT_CHAN3", "SS_BS_VOICE", "SS_PLAYER_VOICE" };

        //Use tf in Hiff Compile and it will handle varients
        public static string[] tf = { "FALSE", "TRUE" };

        public static string[] tfCamel = { "False", "True" };
        public static string[] tfLower = { "false", "true" };

        public static string[] loop = { "LOOP_INFINITE", "LOOP_ONCE" };
        public static string[] execType = { "UNKNOWN", "AE_SINGLE_EXEC" };
        public static string[] CCTEXT_TYPE = { "CCTEXT_TYPE_AUTO", "CCTEXT_TYPE_SCROLL", "CCTEXT_TYPE_SHORT", "CCTEXT_TYPE_NONE" };
        public static string[] depFlag = { "OR_DEPENDENCY_OFF", "OR_DEPENDENCY_ON" };

        public static string[] z = { "Unknown0", "Unknown1", "Unknown2", "Unknown3", "Unknown4", "Unknown5", "Unknown6",
            "Unknown7", "Unknown8", "Unknown9", "VIEWPORT_OVERLAY1_Z", "VIEWPORT_OVERLAY2_Z" };

        public static string[] cursor = { "Unknown0", "MANIPULATE_EXAM_CURSOR", "Unknown2", "Unknown3", "Unknown4", "Unknown5",
            "Unknown6", "Unknown7", "Unknown8", "Unknown9", "BACK_CURSOR", "Unknown11", "FORWARD_CURSOR" };

        public static string[] depType = { "Null", "DT_INVENTORY", "DT_EVENT", "DT_LOGIC",
            "DT_ELAPSED_GAME_TIME", "DT_ELAPSED_SCENE_TIME", "DT_ELAPSED_PLAYER_TIME", "UNKNOWN", "UNKNOWN", "UNKNOWN",
            "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN", "DT_SOUND" };

        public static Dictionary<int, string> ACT_Type =
              new Dictionary<int, string>(){
                   {52, "AT_OVERLAY"},
                   {90, "AT_FLAGS"},
                   {91, "AT_FLAGS_HS"},
                   {111, "AT_POP_SCENE"},
                   {145, "AT_START_SOUND"},
                   {147, "AT_SET_VOLUME"}};

        public static string getCursorTemp(int num)
        {
            if (num == 10)
                return "BACK_CURSOR";
            else if (num == 12)
                return "FORWARD_CURSOR";
            else if (num == 41)
                return "TAKE";
            else
                return num.ToString();
        }
    }
}