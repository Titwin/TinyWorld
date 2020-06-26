using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pathfinding;
using Pathfinding.Util;

public class UnitAI : MonoBehaviour
{
    [Header("UNIT AI")]
    public CharacterAnimationController animator;
    public Unit unit;
    [SerializeField] AIPath ai;
    [SerializeField] List<UnitAction> supportedActions = default;

    [System.Serializable]
    public struct UnitAction : System.IComparable<UnitAction>
    {
        public enum ActionStatus
        {
            Planned, Working, Moving, Ended, Cancelled
        }
        public enum ActionType
        {
            None, Move, AttackUnit, AttackBuilding, ResourceCollect, ResourceDrop, Build, Wander
        }
        [Header("Static properties")]
        // static properties
        public ActionType type;
        public bool run;
        public bool cancellable;
        public float cooldown;
        public float actionMinRange;
        public float actionMaxRange;
        public float priority;

        [Header("Action")]
        public AgentBase target;
        // functional       
        [Header("Dynamic")]
        public ActionStatus status;
        public Vector3Int targetCell;
        public Vector3 targetPosition;
        public Vector3Int interactionCell;
        public Vector3 interactionPosition;
        // score 
        public float distance;
        public float weight;

        public int CompareTo(UnitAction obj)
        {
            return -weight.CompareTo(obj.weight);
        }
    }
    [System.Serializable]
    public class UnitActionRunner
    {
        [SerializeField] internal UnitAction m_action;
        [SerializeField] UnitAI m_controller;
        [SerializeField] AIPath m_navigation;

        [SerializeField] float m_time = 0;
        [SerializeField] float m_wanderDistance = 5;

        BuildingBase building;
        Unit unit;
        ResourceSource resource;

        public UnitActionRunner(UnitAI controller)
        {
            m_controller = controller;
            m_navigation = controller.ai;
        }
        public void Run(UnitAction action)
        {
            if (action.target != null && action.target == m_action.target)
            {
                // continue action;
                m_action.status = UnitAction.ActionStatus.Planned;
                return;
            }
            else
            {
                // reserve a new cell for the interaction
                /*if (m_action.interactionCell != action.interactionCell)
                {
                    World.instance.ReleaseCell(m_controller.unit, m_action.interactionCell);
                    World.instance.ReserveCell(m_controller.unit, action.interactionCell);
                }*/
                m_action = action;
                m_navigation.isStopped = false;
                m_action.status = UnitAction.ActionStatus.Planned;

                m_time = 0;
                if (action.target)
                {
                    building = action.target as BuildingBase;
                    unit = action.target as Unit;
                    resource = action.target as ResourceSource;
                }

            }
        }
        public void FinishAction()
        {
            //World.instance.ReleaseCell(m_controller.unit, m_action.interactionCell);
            m_action.status = UnitAction.ActionStatus.Ended;
        }
        public void CancelAction()
        {
            //World.instance.ReleaseCell(m_controller.unit, m_action.interactionCell);
            m_action.status = UnitAction.ActionStatus.Cancelled;
        }

        public bool UpdateAction(ref UnitAction action, bool current = false)
        {
            Vector3Int workCell;
            bool success = false;
            if (World.instance.ClosestAvailableCell(m_controller.unit, action.target, m_action.interactionCell, out workCell))
            {
                /*if (current && m_action.interactionCell != workCell)
                {
                    World.instance.ReleaseCell(m_controller.unit, m_action.interactionCell);
                    World.instance.ReserveCell(m_controller.unit, workCell);
                }*/
                action.targetCell = action.target.cell;
                action.targetPosition = World.instance.CellToWorld(action.targetCell);
                action.interactionCell = workCell;
                action.interactionPosition = World.instance.CellToWorld(workCell);
                action.distance = World.Distance(m_controller.unit, action.target);
                //action.status = UnitAction.ActionStatus.Planned;
                success = true;
            }
            else
            {
                action.status = UnitAction.ActionStatus.Cancelled;
            }
            return success;
        }
        public void Think()
        {

            if (m_action.target == null)
            {
                m_action.distance = Vector3.Distance(m_controller.transform.position, m_action.interactionPosition);
                if (m_action.status != UnitAction.ActionStatus.Planned && m_navigation.reachedEndOfPath && !m_navigation.pathPending)
                {
                    m_action.status = UnitAction.ActionStatus.Ended;
                }
                else
                {
                    m_action.status = UnitAction.ActionStatus.Moving;
                }
            }
            else //if (m_action.target)
            {
                if (m_controller.InteractionWith(m_action.target) == UnitAction.ActionType.None)
                {
                    m_action.target = null;
                    m_action.status = UnitAction.ActionStatus.Cancelled;
                    m_action.cancellable = true;
                }
                else
                {
                    if (!m_action.target.isStatic)
                    {
                        UpdateAction(ref m_action, true);
                    }
                    m_action.distance = World.Distance(m_controller.unit, m_action.target);
                    if ((m_action.distance < m_action.actionMinRange || m_action.distance > m_action.actionMaxRange) && !m_navigation.pathPending)
                    {
                        m_navigation.isStopped = false;
                        m_action.status = UnitAction.ActionStatus.Moving;
                    }
                    else if (m_action.distance < m_action.actionMaxRange)
                    {
                        m_navigation.isStopped = true;
                        m_action.status = UnitAction.ActionStatus.Working;
                    }
                }
            }
            if (m_action.status == UnitAction.ActionStatus.Moving &&
                m_action.interactionPosition != m_navigation.destination)
            {
                m_navigation.destination = m_action.interactionPosition;
                m_navigation.isStopped = false;
                m_navigation.SearchPath();
            }

        }

        public bool Interruptable()
        {
            return m_action.type == UnitAction.ActionType.None
            //|| m_action.cancellable
            || m_action.status == UnitAction.ActionStatus.Ended
            || m_action.status == UnitAction.ActionStatus.Cancelled;
        }
        public bool Interacting
        {
            get { return m_action.status == UnitAction.ActionStatus.Working; }
        }
        public void Act()
        {
            if (Interacting)
            {
                m_time += Time.deltaTime;
                if (m_time >= m_action.cooldown)
                {
                    PerformAction();
                    m_time -= m_action.cooldown;
                }
            }
        }
        void PerformAction()
        {
            switch (m_action.type)
            {
                case UnitAction.ActionType.None:
                    break;
                case UnitAction.ActionType.Move:
                    break;
                case UnitAction.ActionType.Wander:
                    m_navigation.isStopped = false;
                    m_action.interactionPosition = World.instance.CenteredPosition(new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)) * m_wanderDistance);
                    m_action.interactionCell = World.instance.WorldToCell(m_action.interactionPosition);
                    break;
                case UnitAction.ActionType.Build:
                    m_controller.unit.DoWorkOn(building);
                    if (building.status == BuildingBase.BuildingStatus.Existing)
                    {
                        m_action.status = UnitAction.ActionStatus.Ended;
                    }
                    break;
                case UnitAction.ActionType.ResourceCollect:
                    m_controller.unit.DoWorkOn(resource);
                    if (resource.stock == 0)
                    {
                        m_action.status = UnitAction.ActionStatus.Ended;
                    }
                    break;
                case UnitAction.ActionType.ResourceDrop:
                    m_controller.unit.DoDepositOn(building);
                    break;
                case UnitAction.ActionType.AttackUnit:
                    m_controller.unit.DoAttack(unit);
                    if (!unit.Alive)
                    {
                        m_action.status = UnitAction.ActionStatus.Ended;
                    }
                    break;
                case UnitAction.ActionType.AttackBuilding:
                    m_controller.unit.DoAttack(building);
                    if (building.status == BuildingBase.BuildingStatus.Destroying
                        || building.status == BuildingBase.BuildingStatus.Destroyed)
                    {
                        m_action.status = UnitAction.ActionStatus.Ended;
                    }
                    break;

            }
        }
    }
    /// <summary>Updates the AI's destination every frame</summary>
    /// 
    [Header("Thinking properties")]
    [SerializeField] LayerMask neighborMask;
    //[SerializeField] float runRange = 0;
    [SerializeField] float actionMinRange = 0.1f;
    [SerializeField] float actionMaxRange = 1;
    [SerializeField] float seeRange = 30;

    [Header("Thinking")]
    [SerializeField] int neighborCount;
    [SerializeField] AgentBase[] Neighbors = new AgentBase[64];
    [SerializeField] UnitAction[] actions = new UnitAction[64];
    [SerializeField] UnitActionRunner actionRunner;
    Rect sightArea;

    [Header("Status")]
    [SerializeField] Vector3 direction = Vector3.right;
    Quaternion targetRotation;
    [SerializeField] bool facingForward;
    [SerializeField] float velocity;

    protected void Awake()
    {
        if (!ai) ai = GetComponent<AIPath>();
        unit.StateChanged += OnStateChange;

        ai.maxSpeed = unit.MAX_SPEED;
        ai.maxAcceleration = unit.MAX_ACCELERATION;

        actionMaxRange = unit.weapon.range;
        actionMinRange = unit.weapon.rangeMinimum;

        actionRunner = new UnitActionRunner(this);
        targetRotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    public void OnStateChange()
    {
        if (unit.State == Unit.UnitState.Dead)
        {
            unit.government.RemoveUnit(this);
            ai.isStopped = true;
            ai.enabled = false;
            actionRunner.CancelAction();
        }
        if (unit.State == Unit.UnitState.Free)
        {
            unit.Destroy();
        }
    }
    public void Sense()
    {

        if (!unit.Alive)
        {
            ai.isStopped = true;
            return;
        }
        sightArea = new Rect(transform.position.x - seeRange, transform.position.z - seeRange, seeRange * 2, seeRange * 2);

        if (actionRunner.Interruptable())
        {
            ConsiderActions();
            System.Array.Sort(actions, 0, neighborCount);

            if (actions[0].type != UnitAction.ActionType.None)
            {
                actionRunner.Run(actions[0]);
            }
            else
            {
                // go for default action
            }
        }
    }
    void ConsiderActions()
    {
        neighborCount = World.instance.GetElements(sightArea, ref Neighbors);

        for (int i = neighborCount - 1; i >= 0; --i)
        {
            AgentBase actor = Neighbors[i];
            UnitAction.ActionType actionType = InteractionWith(actor);
            int actionIdx = -1;
            for (int p = 0; p < supportedActions.Count; ++p)
            {
                if (supportedActions[p].type == actionType)
                {
                    actionIdx = p;
                    actions[i] = supportedActions[p];
                }
            }

            bool success = false;
            if (actionIdx >= 0)
            {
                actions[i].target = actor;
                actions[i].cancellable = true;
                success = actionRunner.UpdateAction(ref actions[i]);
            }
            if (!success)
            {
                actions[i].target = actor;
                actions[i].type = UnitAction.ActionType.None;
                actions[i].distance = -float.MaxValue;
                actions[i].priority = -1;
            }
            actions[i].weight = (seeRange - actions[i].distance) * actions[i].priority;
        }
    }

    UnitAction.ActionType InteractionWith(AgentBase other)
    {
        if (other == this)
        {
            return UnitAction.ActionType.None;
        }
        if (!unit.Alive)
        {
            ai.isStopped = true;
            return UnitAction.ActionType.None;
        }
        Unit otherUnit = other as Unit;
        if (otherUnit)
        {
            if (other.team != unit.team && otherUnit.Alive)
            {
                return UnitAction.ActionType.AttackUnit;
            }
        }
        else
        {
            var resource = other as ResourceSource;
            if (resource && resource.stock > 0 && resource.owner == null)
            {
                return UnitAction.ActionType.ResourceCollect;
            }
            else
            {
                var building = other as BuildingBase;
                if (building)
                {
                    if (unit.team == building.team)
                    {
                        if (building.status == BuildingBase.BuildingStatus.Planned || building.status == BuildingBase.BuildingStatus.Constructing)
                        {
                            return UnitAction.ActionType.Build;
                        }
                        else
                        {
                            return UnitAction.ActionType.ResourceDrop;
                        }
                    }
                    else
                    {
                        if ((building.status == BuildingBase.BuildingStatus.Constructing || building.status == BuildingBase.BuildingStatus.Existing))
                        {
                            return UnitAction.ActionType.AttackBuilding;
                        }
                    }
                }
            }
        }
        return UnitAction.ActionType.None;
    }
    public void Think()
    {
        if (!unit.Alive)
        {
            ai.isStopped = true;
            return;
        }
        actionRunner.Think();
    }

    public void Teleport(Vector3 position)
    {
        //animator.controller.enabled = false;
        this.transform.position = position;
        //animator.controller.enabled = true;
    }
    public float angle;
    public void Act()
    {
        if (ai == null)
        {
            return;
        }
        if (!unit.Alive)
        {
            ai.isStopped = true;
            return;
        }
        else
        {
            actionRunner.Act();
        }

        velocity = ai.velocity.magnitude;
        if (velocity > 1f)
        {
            direction = ai.velocity / velocity;
            targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else if (ai.isStopped && (actionRunner.m_action.targetPosition - this.transform.position).sqrMagnitude > 1)
        {
            direction = (actionRunner.m_action.targetPosition - this.transform.position).normalized;
            targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        angle = Quaternion.Angle(this.transform.rotation, targetRotation);
        if (angle < 5)
        {
            facingForward = true;
        }
        else
        {
            facingForward = false;
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRotation, 360 * unit.SPEED_ROTATION * Time.deltaTime);
        }

        animator.SetInput(direction * velocity, velocity > 2.5f, facingForward ? (actionRunner.Interacting ? actionRunner.m_action.target : null) : null);
        unit.OnPostMove();

        animator.Tick();
    }

    public void GoTo(Vector3 position, bool _cancellable = true)
    {
        UnitAction action = new UnitAction()
        {
            target = null,
            interactionPosition = World.instance.CenteredPosition(position),
            interactionCell = World.instance.WorldToCell(position),
            type = UnitAction.ActionType.Move,
            cancellable = _cancellable,
            priority = 1000
        };
        actionRunner.Run(action);
    }
    public void InteractWith(AgentBase target)
    {
        var interaction = InteractionWith(target);
        if (interaction != UnitAction.ActionType.None)
        {
            UnitAction action = new UnitAction()
            {
                target = target,
                type = interaction,
                cancellable = false,
                priority = 1000
            };
            actionRunner.UpdateAction(ref action, true);
            actionRunner.Run(action);
        }
    }


    void OnDrawGizmos2()
    {
        DrawGizmos(false);
    }
    private void OnDrawGizmosSelected2()
    {
        DrawGizmos(true);
    }
    void DrawGizmos(bool verbose = false)
    {
        if (!unit.Alive)
        {
            return;
        }
        //Draw.Gizmos.Hexagon(World.instance.CellToWorld(unit.cell), (unit.radius) / World.instance.circularScale, new Color(1, 1, 1, 0.5f), 90);

        if (actionRunner.m_action.type != UnitAction.ActionType.None)
        {
            // Draw.Gizmos.Hexagon(actionRunner.m_action.interactionPosition, (unit.radius) / World.instance.circularScale, new Color(1, 1, 1, 0.5f), 90);
            Gizmos.color = new Color(1, 1, 1, 0.5f);
            Gizmos.DrawLine(transform.position, actionRunner.m_action.interactionPosition);

            if (ai.Path != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Vector3 previous = Vector3.zero;
                foreach (var current in ai.Path.vectorPath)
                {
                    if (previous != Vector3.zero)
                    {
                        Gizmos.DrawLine(previous, current);
                    }
                    previous = current;

                }
            }
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(this.transform.position, this.transform.position + direction);
    }
}
