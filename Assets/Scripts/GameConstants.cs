/// <summary>
/// Global constants for game configuration
/// Centralized place for all game values to avoid magic numbers
/// </summary>
public static class GameConstants
{
    // Map Boundaries
    public const float MAP_MIN_X = -6f;
    public const float MAP_MAX_X = 6f;
    public const float MAP_MIN_Y = -6f;
    public const float MAP_MAX_Y = 6f;
    public const float MAP_WIDTH = MAP_MAX_X - MAP_MIN_X;
    public const float MAP_HEIGHT = MAP_MAX_Y - MAP_MIN_Y;
    public const float WALL_THICKNESS = 0.2f;
    
    // Enemy spawn boundaries (slightly inside walls)
    public const float ENEMY_SPAWN_MIN_X = -5.5f;
    public const float ENEMY_SPAWN_MAX_X = 5.5f;
    public const float ENEMY_SPAWN_MIN_Y = -5.5f;
    public const float ENEMY_SPAWN_MAX_Y = 5.5f;
    public const float ENEMY_SPAWN_DISTANCE = 3f;
    
    // Player Settings
    public const float PLAYER_SCALE_X = 0.27f;
    public const float PLAYER_SCALE_Y = 0.59f;
    public const float PLAYER_SCALE_Z = 0.39f;
    public const float PLAYER_MOVE_SPEED = 2f;
    public const int PLAYER_MAX_HEALTH = 10;
    
    // Cannon Settings
    public const float CANNON_OFFSET_X = 0.03f / PLAYER_SCALE_X; // Adjusted for player scale
    public const float CANNON_OFFSET_Y = 0.1f / PLAYER_SCALE_Y;  // Adjusted for player scale
    public const float CANNON_FIRE_RATE = 1f;
    public const float CANNON_PROJECTILE_SPAWN_OFFSET = 0.15f; // Distance from cannon to spawn projectile in local space
    
    // Projectile Settings
    public const float PROJECTILE_SCALE = 0.08f;
    public const float PROJECTILE_SPEED = 4f;
    public const float PROJECTILE_LIFETIME = 3f;
    public const int PROJECTILE_DAMAGE = 1;
    
    // Enemy Settings
    public const float ENEMY_SCALE = 0.47f;
    public const float ENEMY_MOVE_SPEED = 0.8f;
    public const int ENEMY_COLLISION_DAMAGE = 1;
    public const float ENEMY_SPAWNED_SCALE_X = 0.007f;
    public const float ENEMY_SPAWNED_SCALE_Y = 0.009f;
    public const float ENEMY_SPAWNED_SCALE_Z = 0.08f;
    
    // Enemy Spawner Settings
    public const float SPAWN_INTERVAL = 2f;
    public const float MIN_SPAWN_INTERVAL = 0.5f;
    public const float LOOT_DROP_CHANCE = 0.2f;
    
    // Loot Settings
    public const float LOOT_GOLD_SCALE_X = 0.008f;
    public const float LOOT_GOLD_SCALE_Y = 0.008f;
    public const float LOOT_GOLD_SCALE_Z = 0.06f;
    public const float LOOT_WOOD_SCALE_X = 0.005f;
    public const float LOOT_WOOD_SCALE_Y = 0.005f;
    public const float LOOT_WOOD_SCALE_Z = 0.06f;
    public const float LOOT_CANVAS_SCALE_X = 0.003f;
    public const float LOOT_CANVAS_SCALE_Y = 0.003f;
    public const float LOOT_CANVAS_SCALE_Z = 0.06f;
    public const float LOOT_METAL_SCALE_X = 0.005f;
    public const float LOOT_METAL_SCALE_Y = 0.005f;
    public const float LOOT_METAL_SCALE_Z = 0.06f;
    public const float LOOT_LIFETIME = 10f;
    
    // Camera Settings
    public const float CAMERA_SMOOTH_SPEED = 0.125f;
    public const float CAMERA_OFFSET_Z = -10f;
}
