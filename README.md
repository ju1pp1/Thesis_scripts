This repository contains the Unity C# scripts developed for my Master’s thesis:
"Designing and Evaluating Context-Aware Artificial Intelligence for Non-Player Characters in Unity-Based Games."

The purpose of this project is to implement and compare two AI architectures for NPC behavior:
Finite State Machine (FSM) – static, rule-based baseline.
Behavior Tree (BT) – hierarchical, context-aware implementation.

Both systems were tested under identical gameplay scenarios and evaluated using logged metrics such as reaction time, decision variance, and context responsiveness.

EnemyBlackboard.cs    -  Shared context data container (distance, health %, line of sight, etc.)
EnemyController.cs    -  High-level control logic
EnemyStats.cs	Health  -  and stat handling
EnemySkillCaster.cs   -  Combat logic
AIEventLogger.cs      -  Logs state transitions and events
AICsvLogger.cs        -  Generates CSV files after play mode
AIMetrics.cs          -  Calculates evaluation metrics

FSM Implementation

The FSM system consists of:
EnemyStateMachine.cs
IEnemyState interface
Individual state classes (PatrolState, AttackState, FleeState, etc.)

Lifecycle methods:
OnEnter()
Tick()
OnExit()

The FSM evaluates transitions per frame and executes one active state at a time.

Behavior Tree Implementation

The BT system consists of:
EnemyBehaviorTree.cs

Node types:
Selector
Sequence
Condition
Action

Node Return States:
Success
Failure
Running

👤 Author
Jeremi Andersin,
Master’s Degree Programme in Computing Sciences,
Tampere University,
2026
