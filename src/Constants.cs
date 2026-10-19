using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RISKHackTool.src
{
    public static class BaseOffsets
    {
        public const int GAME_MANAGER_TYPE_INFO_OFFSET = 0x327BDD8;

        public const int MAP_MANAGER_TYPE_INFO_OFFSET = 0x3207DC0;
    }

    public static class Constants
    {
        public const int MAX_BUFFER_SIZE = 100;
        public const int MAX_SCROLL_SIZE = 5000;
    }

    public static class GameManagerOffsets
    {
        // All offsets from dump.cs - DO NOT CHANGE
        public const int GAME_MODE_OFFSET = 0x0;
        public const int BOT_DIFFICULTY_OFFSET = 0x4;
        public const int TURN_TIME_OFFSET = 0x8;
        public const int DYNAMIC_TURN_TIME_OFFSET = 0xC;
        public const int IS_AUTO_SETUP_OFFSET = 0xD;
        public const int CLAIM_TYPE_OFFSET = 0x10;
        public const int IS_FORTIFY_ONCE_OFFSET = 0x14;
        public const int IS_FOG_OF_WAR_OFFSET = 0x15;
        public const int IS_BLOCKERS_OFFSET = 0x16;
        public const int IS_ALLIANCE_OFFSET = 0x17;
        public const int IS_SECRET_MISSION_OFFSET = 0x18;
        public const int IS_RANKED_OFFSET = 0x19;
        public const int MINIMUM_RANK_OFFSET = 0x1C;
        public const int MAXIMUM_RANK_OFFSET = 0x20;
        public const int DICE_ROLLS_OFFSET = 0x24;
        public const int MAX_TROOPS_OFFSET = 0x28;
        public const int PORTALS_OFFSET = 0x2C;
        public const int INACTIVITY_BEHAVIOR_OFFSET = 0x30;
        public const int PORTALS_SHIFT_OFFSET = 0x34;
        public const int PORTAL_NUMBER_OFFSET = 0x38;
        public const int MULTIPLAYER_VERSION_OFFSET = 0x40;
        public const int ROUND_LIMIT_OFFSET = 0x48;
        public const int CURRENT_ROUND_OFFSET = 0x4C;
        public const int IS_PROCESSING_RESULTS_OFFSET = 0x50;
        public const int DELAYED_HIGHLIGHTS_OFFSET = 0x51;
        public const int HAS_STARTED_OFFSET = 0x52;
        public const int HAS_ENDED_OFFSET = 0x53;
        public const int GAME_STATE_TYPE_OFFSET = 0x54;
        public const int GAME_STATE_SETUP_OFFSET = 0x58;
        public const int GAME_STATE_PLAYERS_OFFSET = 0x60;
        public const int GAME_STATE_SELECTION_OFFSET = 0x68;
        public const int IS_PAUSED_OFFSET = 0x70;
        public const int GAME_ENDED_OFFSET = 0x71;
        public const int SEQUENCE_CALL_ID_OFFSET = 0x74;
        public const int SEQUENCE_GAME_TIME_OFFSET = 0x78;
        public const int PLAYER_POSITIONS_OFFSET = 0x80;
        public const int TERRITORY_CARD_CONFIG_OFFSET = 0x88;
        public const int STARTING_UNITS_CONFIG_OFFSET = 0xE0;
        public const int OPT_IN_TUTORIAL_OFFSET = 0xFC;
        public const int GAME_TYPE_OFFSET = 0x100;
        public const int LAST_GAME_TYPE_OFFSET = 0x104;
        public const int MODE_OFFSET = 0x108;
        public const int PLAYERS_OFFSET = 0x110;
        public const int GAME_CONTROL_OFFSET = 0x118;
        public const int SESSION_OFFSET = 0x120;
        public const int GAME_DATA_OFFSET = 0x128;
        public const int SANDBOX_DATA_OFFSET = 0x130;
        public const int AI_ASSISTANT_OFFSET = 0x210;
        public const int ON_CREATE_SUCCESS_OFFSET = 0x218;
        public const int ON_CREATE_ERROR_OFFSET = 0x220;
        public const int ON_JOIN_SUCCESS_OFFSET = 0x228;
        public const int ON_JOIN_ERROR_OFFSET = 0x230;
        public const int ON_CONFIRM_QUIT_OFFSET = 0x238;
        public const int IS_FORCE_QUITTING_OFFSET = 0x240;
        public const int ON_GAME_MODE_CHANGED_OFFSET = 0x248;
        public const int ACTIVE_SECRET_MISSION_OVERLAY_REGIONS_OFFSET = 0x250;
        public const int ACTIVE_SECRET_MISSION_OVERLAY_PLAYER_OFFSET = 0x258;
        public const int ACTIVE_SECRET_MISSION_OVERLAY_MISSION_OFFSET = 0x25C;
        public const int VALID_GAME_MODES_OFFSET = 0x260;
        public const int VALID_GAME_MODE_FILTERS_OFFSET = 0x268;
        public const int SECRET_MISSION_IDS_OFFSET = 0x270;
        public const int PLAYER_INDEX_FOR_COLOUR_OFFSET = 0x278;
        public const int AVAILABLE_MISSION_IDS_OFFSET = 0x280;
    }

    public static class GameManagerConfigOffsets
    {
        public const int FOG_ENABLED_OFFSET = 0x15;
    }

    public static class TerritoriesListOffsets
    {
        public const int SIZE_OFFSET = 0x18;
        public const int FIRST_TERRITORY_OFFSET = 0x20;
    }

    public static class TerritoryOffsets
    {
        public const int ID_OFFSET = 0x20;
        public const int REGION_OFFSET = 0x28;
        public const int MIRROR_COUNTRY_OFFSET = 0x30;
        public const int LINKS_OFFSET = 0x38;
        public const int START_POSITION_OFFSET = 0x40;
        public const int TROOP_POSITIONS_OFFSET = 0x50;
        public const int MESH_FILTER_TILING_OFFSET = 0x58;
        public const int MESH_FILTER_HIGHLIGHT_OFFSET = 0x60;
        public const int MAP_OFFSET = 0x68;
        public const int TERRITORY_INFO_OFFSET = 0x70;
        public const int IS_INITIALIZED_OFFSET = 0x78;
        public const int PREV_PLAYER_OFFSET = 0x80;
        public const int PLAYER_OFFSET = 0x88;
        public const int ENCRYPTED_UNITS_OFFSET = 0x90;
        public const int PENDING_UNITS_OFFSET = 0xA0;
        public const int PREV_UNITS_OFFSET = 0xB0;
        public const int IS_CAPITAL_OFFSET = 0xB4;
        public const int CAPITAL_HIGHLIGHT_OFFSET = 0xB5;
        public const int IS_BLOCKER_OFFSET = 0xB6;
        public const int PORTAL_STATE_OFFSET = 0xB8;
        public const int UNITS_BATTLE_MOVE_MAX_OFFSET = 0xBC;
        public const int UNITS_BATTLE_COUNTRY_BASE_OFFSET = 0xC0;
        public const int PORTAL_OFFSET = 0xC8;
        public const int PORTAL_SCALE_OFFSET = 0xD0;
        public const int IS_PORTAL_ANIMATING_OFFSET = 0xD4;
        public const int PORTALS_MIN_SCALE_OFFSET = 0xD8;
        public const int MAP_LABEL_OFFSET = 0xE8;
        public const int HIGHLIGHT_RENDERER_OFFSET = 0xF0;
        public const int TILING_RENDERER_OFFSET = 0xF8;
        public const int HIGHLIGHT_PROPERTY_BLOCK_OFFSET = 0x100;
        public const int TILING_PROPERTY_BLOCK_OFFSET = 0x108;
        public const int DECAL_OFFSET_OFFSET = 0x110;
        public const int PORTAL_HAS_ANIMATED_OFFSET = 0x118;
        public const int CAN_BE_SELECTED_OFFSET = 0x119;
        public const int NAME_OFFSET = 0x120;
        public const int NAME_DE_OFFSET = 0x128;
        public const int NAME_FR_OFFSET = 0x130;
        public const int NAME_ES_OFFSET = 0x138;
        public const int NAME_IT_OFFSET = 0x140;
        public const int NAME_PT_OFFSET = 0x148;
        public const int NAME_RU_OFFSET = 0x150;
        public const int NAME_TR_OFFSET = 0x158;
        public const int NAME_ZH_OFFSET = 0x160;
        public const int IS_SHOWING_TERRITORY_OFFSET = 0x168;
        public const int MESH_FILTER_EDGE_OFFSET = 0x170;
        public const int LAST_COLOR_BORDER_OFFSET = 0x178;
        public const int TILING_COLOR_TWEEN_OFFSET = 0x188;
        public const int TARGET_HIGHLIGHT_COLOR_OFFSET = 0x190;
        public const int SET_HIGHLIGHT_COLOR_OFFSET = 0x1A0;
        public const int TARGET_TILING_COLOR_OFFSET = 0x1B0;
        public const int SET_TILING_COLOR_OFFSET = 0x1C0;
        public const int TILING_COLOR_TWEEN_START_OFFSET = 0x1D0;
        public const int TILING_COLOR_TWEEN_TARGET_OFFSET = 0x1E0;
        public const int HIGHLIGHT_GRID_COLOR_OFFSET = 0x1F0;
        public const int HIGHLIGHT_GRID_INTENSITY_OFFSET = 0x200;
        public const int IS_MIRROR_DISPLAYING_CONTINENT_COLOR_OFFSET = 0x204;
        public const int IS_DISPLAYING_CONTINENT_COLOR_OFFSET = 0x205;
        public const int DISABLE_UPDATE_COLOR_OFFSET = 0x206;
        public const int IS_COLOR_BLEEDING_OFFSET = 0x207;
        public const int BLEED_COLOR_OFFSET = 0x208;
        public const int TWEEN_BLEED_SCALE_X_OFFSET = 0x218;
        public const int TWEEN_BLEED_SCALE_Z_OFFSET = 0x220;
        public const int HOVERED_OFFSET = 0x228;
        public const int SELECTED_OFFSET = 0x229;
        public const int HIGHLIGHTED_OFFSET = 0x22A;
        public const int BREATHING_OFFSET = 0x22B;
        public const int GRID_OFFSET = 0x22C;
        public const int MAP_LABEL_TWEEN_OFFSET = 0x230;
        public const int HEIGHT_TWEEN_OFFSET = 0x238;
        public const int SCALE_TWEEN_OFFSET = 0x240;
        public const int CURRENT_HEIGHT_OFFSET = 0x248;

        public const int TERRITORY_TYPE_OFFSET = 0xB4;
    }

    public static class TerritoryInfoOffsets
    {
        public const int NAME_OFFSET = 0x18;
    }

    public static class RegionOffsets
    {
        public const int REGION_INFO_OFFSET = 0x48;
    }

    public static class RegionInfoOffsets
    {
        public const int NAME_OFFSET = 0x18;
    }

    public static class PlayerOffsets
    {
        public const int PLAYER_COLOR_OFFSET = 0xD0;
        public const int PLACEABLE_TROOPS_OFFSET = 0xD8;
        public const int CARD_LIST_OFFSET = 0x110;

        public const int COLOR_OFFSET = 0xD0;
    }

    public static class CardListOffsets
    {
        public const int PHYSICAL_CARD_LIST_OFFSET = 0x10;
        public const int NUMBER_OF_CARDS_OFFSET = 0x18;
    }

    public static class PhysicalCardListOffsets
    {
        public const int FIRST_CARD_OFFSET = 0x20;
    }

    public static class CardOffsets
    {
        public const int CARD_TYPE_OFFSET = 0x10;
        public const int CARD_TERRITORY_OFFSET = 0x18;
    }

    public static class StringOffsets
    {
        public const int SIZE_OFFSET = 0x10;
        public const int FIRST_CHAR_OFFSET = 0x14;
    }

    public static class MemoryConstants
    {
        public const int BOOLEAN_BYTES = 0x1;
        public const int INT_BYTES = 0x4;
        public const int POINTER_BYTES = 0x8;
    }

    public static class CSVConstants
    {
        public const int TERRITORY_NAME = 0;
        public const int COLOR = 1;
        public const int TERRITORY_TYPE = 2;
        public const int TROOP_COUNT = 3;
        public const int NUM_COLS = 4;
    }

    enum TerritoryType
    {
        Regular = 0,
        Capital = 1,
        Blizzard = 256
    }

    enum CardType
    {
        Any = 0,
        Infantry = 1,
        Calvary = 2,
        Artillery = 3
    }

    enum FogConfig
    {
        Disabled = 0
    }
}