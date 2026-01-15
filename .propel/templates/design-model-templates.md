# Design Modelling

## UML Models Overview
[Short description about the need of the UML diagrams]

## Architectural Views

### System Context Diagram
```plantuml
- [UML representation of system's boundary, its main function, and its interactions with external entities (users, other systems) via data flows.]
```

### Component Architecture Diagram
```mermaid
- [UML representation of software system by breaking it down into individual components or modules.]
```

### Deployment Architecture Diagram
```plantuml
- [Visual overview of the Cloud landing zone architecture, highlighting core pillars (identity, networking, security, governance, and operations), shared services, and workload domains, along with data/control-plane boundaries.]
```

### Data Flow Diagram
```plantuml
- [Visual representation of where data comes from (sources), how it is processed (transformations), and where it is stored (data stores). ]
```

### Logical Data Model (ERD)
```mermaid
[Visual representation of entities, attributes and relationships.]
```

### Use Case Sequence Diagrams

> **Note**: Create one sequence diagram for each use case (UC-XXX) defined in `.propel/context/docs/spec.md` or `.propel/context/docs/codeanalysis.md`. Each sequence diagram details the dynamic message flow and timing for its corresponding use case. Do NOT duplicate the use case diagrams (those remain in spec.md/codeanalysis.md only).

#### UC-001: [Use Case Name]
**Source**: [spec.md#UC-001](.propel/context/docs/spec.md#UC-001) or [codeanalysis.md#UC-001](.propel/context/docs/codeanalysis.md#UC-001)

```mermaid
sequenceDiagram
    participant Actor as [Actor Name]
    participant System as [System Component]
    participant Backend as [Backend Service]
    participant Database as [Data Store]

    Note over Actor,Database: UC-001 - [Use Case Name]

    Actor->>System: [Step 1 from use case]
    System->>Backend: [Internal processing]
    Backend->>Database: [Data operation]
    Database-->>Backend: [Data response]
    Backend-->>System: [Processing result]
    System-->>Actor: [Final outcome]
```

#### UC-002: [Use Case Name]
**Source**: [spec.md#UC-002](.propel/context/docs/spec.md#UC-002) or [codeanalysis.md#UC-002](.propel/context/docs/codeanalysis.md#UC-002)

```mermaid
sequenceDiagram
    participant Actor as [Actor Name]
    participant System as [System Component]

    Note over Actor,System: UC-002 - [Use Case Name]

    Actor->>System: [Interaction steps]
    System-->>Actor: [Response]
```

[Repeat for each UC-XXX found in spec.md or codeanalysis.md]
