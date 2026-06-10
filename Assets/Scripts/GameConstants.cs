/// <summary>
/// Global constants for game configuration
/// Centralized place for all game values to avoid magic numbers
/// </summary>
public static class GameConstants
{
    // Map Boundaries (Biome 1 is 250x250 — a real voyage, ~2 min to cross)
    public const float MAP_MIN_X = -125f;
    public const float MAP_MAX_X = 125f;
    public const float MAP_MIN_Y = -125f;
    public const float MAP_MAX_Y = 125f;

    // Home port (Safe Harbor) position — player respawns here
    public const float HOME_PORT_X = 12.5f;
    public const float HOME_PORT_Y = 12.5f;
    public const float MAP_WIDTH = MAP_MAX_X - MAP_MIN_X;
    public const float MAP_HEIGHT = MAP_MAX_Y - MAP_MIN_Y;
    public const float WALL_THICKNESS = 0.5f;
    
    // Enemy spawn boundaries (clamp to keep spawns inside map)
    public const float ENEMY_SPAWN_MIN_X = MAP_MIN_X + 2f;
    public const float ENEMY_SPAWN_MAX_X = MAP_MAX_X - 2f;
    public const float ENEMY_SPAWN_MIN_Y = MAP_MIN_Y + 2f;
    public const float ENEMY_SPAWN_MAX_Y = MAP_MAX_Y - 2f;
    // Must exceed the camera half-diagonal (~16 at orthoSize 8, 16:9) so
    // enemies always spawn OFF-SCREEN and sail/swim into view.
    public const float ENEMY_SPAWN_DISTANCE = 17f;
    
    // Player Settings
    public const float PLAYER_SCALE_X = 0.54f;  // 0.27 * 2
    public const float PLAYER_SCALE_Y = 1.18f;  // 0.59 * 2
    public const float PLAYER_SCALE_Z = 0.78f;  // 0.39 * 2
    public const float PLAYER_MOVE_SPEED = 2f;
    public const int PLAYER_MAX_HEALTH = 10;
    
    // Cannon Settings
    public const float CANNON_OFFSET_X = 0.06f / PLAYER_SCALE_X; // 0.03 * 2, adjusted for player scale
    public const float CANNON_OFFSET_Y = 0.2f / PLAYER_SCALE_Y;  // 0.1 * 2, adjusted for player scale
    public const float CANNON_FIRE_RATE = 1f;
    public const float CANNON_PROJECTILE_SPAWN_OFFSET = 0.3f;  // 0.15 * 2
    
    // Projectile Settings
    public const float PROJECTILE_SCALE = 0.4f;
    public const float PROJECTILE_SPEED = 4f;
    public const float PROJECTILE_LIFETIME = 3f;
    public const int PROJECTILE_DAMAGE = 1;
    
    // Enemy Settings
    public const float ENEMY_SCALE = 0.94f;  // 0.47 * 2
    public const float ENEMY_MOVE_SPEED = 0.8f;
    public const int ENEMY_COLLISION_DAMAGE = 1;
    public const float ENEMY_SPAWNED_SCALE_X = 0.014f;  // 0.007 * 2
    public const float ENEMY_SPAWNED_SCALE_Y = 0.018f;  // 0.009 * 2
    public const float ENEMY_SPAWNED_SCALE_Z = 0.16f;   // 0.08 * 2
    public const float ENEMY_TARGET_HEIGHT = 0.5f;       // world-units tall for monster enemies (new sprites)
    public const float ENEMY_SHIP_TARGET_HEIGHT = 0.85f; // world-units tall for enemy ships (new sprite)
    
    // Enemy Spawner Settings
    public const float SPAWN_INTERVAL = 2f;
    public const float MIN_SPAWN_INTERVAL = 0.5f;
    public const float LOOT_DROP_CHANCE = 0.2f;
    
    // Loot Settings (loot sprites are 20.48 world units at scale 1, so 0.03 ~= 0.6 wu)
    public const float LOOT_GOLD_SCALE_X = 0.03f;
    public const float LOOT_GOLD_SCALE_Y = 0.03f;
    public const float LOOT_GOLD_SCALE_Z = 0.12f;
    public const float LOOT_WOOD_SCALE_X = 0.025f;
    public const float LOOT_WOOD_SCALE_Y = 0.025f;
    public const float LOOT_WOOD_SCALE_Z = 0.12f;
    public const float LOOT_CANVAS_SCALE_X = 0.025f;
    public const float LOOT_CANVAS_SCALE_Y = 0.025f;
    public const float LOOT_CANVAS_SCALE_Z = 0.12f;
    public const float LOOT_METAL_SCALE_X = 0.025f;
    public const float LOOT_METAL_SCALE_Y = 0.025f;
    public const float LOOT_METAL_SCALE_Z = 0.12f;
    public const float LOOT_LIFETIME = 10f;
    
    // Enemy Ship Settings
    public const float ENEMY_SHIP_MOVE_SPEED = 1.2f;
    public const float ENEMY_SHIP_CIRCLE_RADIUS = 4f;
    public const int ENEMY_SHIP_HP = 5;
    public const int ENEMY_SHIP_COLLISION_DAMAGE = 2;
    public const float ENEMY_SHIP_FIRE_RATE = 2f;
    public const float ENEMY_SHIP_LOOT_DROP_CHANCE = 0.4f;
    public const float ENEMY_SHIP_PROJECTILE_SPEED = 3f;
    public const int ENEMY_SHIP_PROJECTILE_DAMAGE = 1;
    public const float ENEMY_SHIP_SPAWNED_SCALE_X = 0.43f;
    public const float ENEMY_SHIP_SPAWNED_SCALE_Y = 0.94f;
    public const float ENEMY_SHIP_SPAWNED_SCALE_Z = 0.62f;

    // Camera Settings
    public const float CAMERA_SMOOTH_SPEED = 0.125f;
    public const float CAMERA_OFFSET_Z = -10f;
}
