using Godot;
using System;
using System.Data.SQLite;

public partial class DatabaseManager : Node
{
	private static DatabaseManager _instance;
	public static DatabaseManager Instance => _instance;

	private SQLiteConnection db;
	private string dbPath;

	public override void _Ready()
	{
		_instance = this;
		InitializeDatabase();
	}

	private void InitializeDatabase()
	{
		dbPath = ProjectSettings.GlobalizePath("user://battle_data.db"); // 所有模式共用一个DB文件
		db = new SQLiteConnection($"Data Source={dbPath};Version=3;");
		db.Open();
		CreateTables();
		GD.Print($"数据库初始化完成: {dbPath}");
	}

	private void CreateTables()
	{
		string createTableQuery = @"
			CREATE TABLE IF NOT EXISTS player_stats (
				id INTEGER PRIMARY KEY AUTOINCREMENT,
				player_name TEXT NOT NULL UNIQUE,
				hp INTEGER NOT NULL,
				mp INTEGER NOT NULL,
				max_hp INTEGER NOT NULL,
				max_mp INTEGER NOT NULL,
				last_updated DATETIME DEFAULT CURRENT_TIMESTAMP
			)";
		using (var cmd = new SQLiteCommand(createTableQuery, db))
		{
			cmd.ExecuteNonQuery();
		}
	}

	// 新增：初始化玩家（如不存在时）
	public void EnsurePlayerStats(string playerName, int hp, int mp, int maxHp, int maxMp)
	{
		string checkQuery = "SELECT COUNT(*) FROM player_stats WHERE player_name = @name";
		using (var cmd = new SQLiteCommand(checkQuery, db))
		{
			cmd.Parameters.AddWithValue("@name", playerName);
			int count = Convert.ToInt32(cmd.ExecuteScalar());
			if (count == 0)
			{
				SavePlayerStats(playerName, hp, mp, maxHp, maxMp);
			}
		}
	}

	public void SavePlayerStats(string playerName, int hp, int mp, int maxHp, int maxMp)
	{
		try
		{
			string query = @"
				INSERT INTO player_stats (player_name, hp, mp, max_hp, max_mp, last_updated)
				VALUES (@name, @hp, @mp, @maxHp, @maxMp, CURRENT_TIMESTAMP)
				ON CONFLICT(player_name) DO UPDATE SET
					hp = excluded.hp,
					mp = excluded.mp,
					max_hp = excluded.max_hp,
					max_mp = excluded.max_mp,
					last_updated = CURRENT_TIMESTAMP";
			using (var cmd = new SQLiteCommand(query, db))
			{
				cmd.Parameters.AddWithValue("@name", playerName);
				cmd.Parameters.AddWithValue("@hp", hp);
				cmd.Parameters.AddWithValue("@mp", mp);
				cmd.Parameters.AddWithValue("@maxHp", maxHp);
				cmd.Parameters.AddWithValue("@maxMp", maxMp);
				cmd.ExecuteNonQuery();
				GD.Print($"已保存 {playerName} 数据: HP={hp}, MP={mp}");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"保存玩家数据失败: {ex.Message}");
		}
	}

	public PlayerStats LoadPlayerStats(string playerName, int defaultHp = 10, int defaultMp = 0, int defaultMaxHp = 10, int defaultMaxMp = 15)
	{
		try
		{
			string query = "SELECT * FROM player_stats WHERE player_name = @name LIMIT 1";
			using (var cmd = new SQLiteCommand(query, db))
			{
				cmd.Parameters.AddWithValue("@name", playerName);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return new PlayerStats
						{
							PlayerName = playerName,
							HP = Convert.ToInt32(reader["hp"]),
							MP = Convert.ToInt32(reader["mp"]),
							MaxHP = Convert.ToInt32(reader["max_hp"]),
							MaxMP = Convert.ToInt32(reader["max_mp"])
						};
					}
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"加载玩家数据失败: {ex.Message}");
		}
		// 默认值
		return new PlayerStats
		{
			PlayerName = playerName,
			HP = defaultHp,
			MP = defaultMp,
			MaxHP = defaultMaxHp,
			MaxMP = defaultMaxMp
		};
	}

	// 用于一键保存一场对战双方数据
	public void SaveBattleState(string player1Name, int p1Hp, int p1Mp, int p1MaxHp, int p1MaxMp,
							   string player2Name, int p2Hp, int p2Mp, int p2MaxHp, int p2MaxMp)
	{
		SavePlayerStats(player1Name, p1Hp, p1Mp, p1MaxHp, p1MaxMp);
		SavePlayerStats(player2Name, p2Hp, p2Mp, p2MaxHp, p2MaxMp);
	}

	public override void _ExitTree()
	{
		db?.Close();
	}
}
public class PlayerStats
{
	public string PlayerName { get; set; }
	public int HP { get; set; }
	public int MP { get; set; }
	public int MaxHP { get; set; }
	public int MaxMP { get; set; }
}
