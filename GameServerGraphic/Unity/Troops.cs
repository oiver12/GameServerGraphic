using GameServer;
using System;

[Flags]
public enum TroopClass
{
	Tank = (1 << 0),
	Commander = (1 << 1),
	Archer = (1 << 2),
}

//[CreateAssetMenu(fileName = "New Troop", menuName = "Troops")]
[System.Serializable]
public class Troops
{
	//im Spiel können die Variablen nicht mehr geändert werden
	//[SerializeField] private int m_id;
	//public int id { get { return m_id; } private set { m_id = value; } }
	public readonly int id;

	//[SerializeField] private string m_name;
	//public string Name { get { return m_name; } private set { m_name = value; } }
	public readonly string Name;

	//[SerializeField] [EnumFlags] public TroopClass m_klasse;
	//public TroopClass klasse { get { return m_klasse; } private set { m_klasse = value; } }
	public readonly TroopClass klasse;

	//[SerializeField] private float m_maxHealth;
	//public float maxHealth { get { return m_maxHealth; } private set { m_maxHealth = value; } }
	public readonly float maxHealth;

	//[SerializeField] private float m_attackSpeed;
	//public float attackSpeed { get { return m_attackSpeed; } private set { m_attackSpeed = value; } }
	public readonly float attackSpeed;

	//[SerializeField] private float m_moveSpeed;
	//public float moveSpeed { get { return m_moveSpeed; } private set { m_moveSpeed = value; } }
	public readonly float moveSpeed;

	//[SerializeField] private float m_placeRadius;
	//public float placeRadius { get { return m_placeRadius; } private set { m_placeRadius = value; } }
	public readonly float placeRadius;

	//[SerializeField] private GameObject m_objectToPlace;
	//public GameObject objectToPlace { get { return m_objectToPlace; } private set { m_objectToPlace = value; } }
	//public readonly Prefab objectToPlace;

	//[SerializeField] private float m_damage;
	//public float damage { get { return m_damage; } private set { m_damage = value; } }
	public readonly float damage;

	//[SerializeField] private float m_attackRadius;
	//public float attackRadius { get { return m_attackRadius; } private set { m_attackRadius = value; } }
	public readonly float attackRadius;

	//[SerializeField] private int m_maxTroops;
	//public int maxTroops { get { return m_maxTroops; } private set { m_maxTroops = value; } }
	public readonly int maxTroops;

	public Troops(int id, string name, TroopClass klasse, float maxHealth, float attackSpeed, float moveSpeed, float placeRadius, float damage, float attackRadius, int maxTroops)
	{
		this.id = id;
		Name = name;
		this.klasse = klasse;
		this.maxHealth = maxHealth;
		this.attackSpeed = attackSpeed;
		this.moveSpeed = moveSpeed;
		this.placeRadius = placeRadius;
		this.damage = damage;
		this.attackRadius = attackRadius;
		this.maxTroops = maxTroops;
	}

	private Troops() { }
}

public static class EnumExtension
{
	public static bool IsFlagSet(this TroopClass value, TroopClass flag)
	{
		return (value & flag) == flag;
	}
}
