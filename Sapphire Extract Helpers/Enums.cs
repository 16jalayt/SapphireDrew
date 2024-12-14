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
        public static string[] difficulty = { "DIFFICULTY_LEVEL_EASY", "UnknownDifficutly", "DIFFICULTY_LEVEL_HARD" };
        public static string[] tod = { "PLAYER_DAY", "PLAYER_NIGHT" };

        //TODO:values
        public static string[] value = { "ADD_TO_VALUE", "TEST_ACTUAL_VALUE" };

        public static string[] z = { "Unknown0", "Unknown1", "Unknown2", "Unknown3", "Unknown4", "Unknown5", "Unknown6",
            "Unknown7", "Unknown8", "Unknown9", "VIEWPORT_OVERLAY1_Z", "VIEWPORT_OVERLAY2_Z" };

        public static string[] cursor = { "Unknown0", "MANIPULATE_EXAM_CURSOR", "Unknown2", "Unknown3", "Unknown4", "Unknown5",
            "Unknown6", "Unknown7", "Unknown8", "Unknown9", "BACK_CURSOR", "Unknown11", "FORWARD_CURSOR" };

        public static string[] timers = { "GAMETIMER_0", "GAMETIMER_1", "GAMETIMER_2", "GAMETIMER_3", "GAMETIMER_4", "GAMETIMER_5",
            "GAMETIMER_6", "GAMETIMER_7", "GAMETIMER_8", "GAMETIMER_9", "GAMETIMER_10", "GAMETIMER_11", "GAMETIMER_12",
            "GAMETIMER_13", "GAMETIMER_14", "GAMETIMER_15", "GAMETIMER_16", "GAMETIMER_17", "GAMETIMER_18", "GAMETIMER_19"};

        public static string[] depType = { "Null", "DT_INVENTORY", "DT_EVENT", "DT_LOGIC",
            "DT_ELAPSED_GAME_TIME", "DT_ELAPSED_SCENE_TIME", "DT_ELAPSED_PLAYER_TIME", "DT_SAMS_SIGHT", "DT_SAMS_SOUND", "DT_SCENE_COUNT",
            "DT_ELAPSED_PLAYER_DAY", "DT_CURSOR_TYPE", "DT_PLAYER_TOD", "DT_TIMER_LESS_THAN_DEPENDENCY_TIME", "DT_TIMER_GREATER_THAN_DEPENDENCY_TIME",
            "DT_DIFFICULTY_LEVEL", "DT_CLOSED_CAPTIONING", "DT_SOUND", "DT_OPEN_PARENTHESIS", "DT_CLOSE_PARENTHESIS", "DT_RANDOM", "DT_DEFAULT_AR" };

        //TODO:Table index 0-40

        public static Dictionary<int, string> ACT_Type =
              new Dictionary<int, string>(){
                  {16, "AT_SCENE_FRAME"},
                  {19, "AT_SCENE_FRAME_HS"},
                   {29, "AT_CONTROL_UI"},
                   {52, "AT_OVERLAY"},
                   {90, "AT_FLAGS"},
                   {91, "AT_FLAGS_HS"},
                   {102, "AT_SAVE_CONTINUE_GAME"},
                   {111, "AT_POP_SCENE"},
                   {145, "AT_START_SOUND"},
                   {147, "AT_SET_VOLUME"}};

        public static Dictionary<int, string> cursorDict =
              new Dictionary<int, string>(){
                  {10, "BACK_CURSOR"},
                  {12, "FORWARD_CURSOR"},
                   {19, "UTURN_CURSOR"},
                   {22, "MANIPULATE_EXAM_CURSOR"},
                   {41, "TAKE"}};

        public static string getCursorTemp(int num)
        {
            if (num == 10)
                return "BACK_CURSOR";
            else if (num == 12)
                return "FORWARD_CURSOR";
            else if (num == 19)
                return "UTURN_CURSOR";
            else if (num == 22)
                return "MANIPULATE_EXAM_CURSOR";
            else if (num == 41)
                return "TAKE";
            else
                return num.ToString();
        }
    }
}