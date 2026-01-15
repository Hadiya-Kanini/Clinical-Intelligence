---
trigger: glob
globs: "**/{spec.md,design.md,codeanalysis.md,requirements.md}"
---

# UML Text-Based Code Standards

PlantUML and Mermaid diagram standards: clarity, consistency, single-page focus.

## Core Principles

- One diagram = one story; max 6-8 containers/components and 6-8 externals/actors
- Reuse same visual elements for same roles across diagrams
- Move complex internals to separate lower-level diagrams

## 1. Boundary Scopes

- Nest: Context → System → Container/Component (max 3 levels)
- Group only related elements; label with purpose

**PlantUML:**
```plantuml
package "System Boundary" {
  component "Component A"
  component "Component B"
}
```

**Mermaid:**
```mermaid
subgraph "System Boundary"
  ComponentA[Component A]
  ComponentB[Component B]
end
```

## 2. Containers / Components

- One container = one deployable unit with single responsibility
- Label: `Name (Technology) - Responsibility` (max 1 line)
- Reuse same symbol for same role

**Examples:**
- ✓ `API Gateway (Kong) - Routes & authenticates requests`
- ✓ `User Service - Manages user profiles`
- ✗ `API Gateway (Kong) - Routes requests, authenticates users, handles rate limiting, logs traffic`

## 3. Colors & Styles

Use color to encode type (max 4-5 colors):
| Type | Color/Style | Usage |
|------|-------------|-------|
| Actor | Blue | Human users, external systems |
| Core System | Green | Primary application components |
| External System | Gray | Third-party services, legacy systems |
| Data Store | Yellow | Databases, caches, message queues |
| Tooling/DevOps | Orange | CI/CD, monitoring, infrastructure |

**Line Styles:** Solid = internal/sync; Dashed = external/async

**PlantUML:**
```plantuml
actor User #LightBlue
component "Core API" #LightGreen
database "PostgreSQL" #Yellow
component "External Service" #LightGray

User --> "Core API" : HTTPS
"Core API" ..> "External Service" : Async
```

**Mermaid:**
```mermaid
graph LR
  User[User]:::actor
  API[Core API]:::core
  DB[(PostgreSQL)]:::data
  Ext[External Service]:::external

  User -->|HTTPS| API
  API -.->|Async| Ext

  classDef actor fill:#add8e6
  classDef core fill:#90ee90
  classDef data fill:#ffffe0
  classDef external fill:#d3d3d3
```

## 4. Layout & Format

- Flow: Left → Right or Top → Bottom (consistent)
- Align tiers horizontally (UI, API, Data, Integration)
- Labels: max 50 chars

**PlantUML:**
```plantuml
left to right direction
' or
top to bottom direction

skinparam linetype ortho
```

**Mermaid:**
```mermaid
graph LR  %% Left to Right
' or
graph TB  %% Top to Bottom
```

## 5. Connectors

- Straight, shortest-path arrows; avoid crossing
- Use hub node for fan-out scenarios
- Label: `Protocol / Type - Purpose` (max 1 line)

**Examples:**
- ✓ `HTTPS / REST - Get profile`
- ✓ `Message Queue - Order created event`
- ✗ `HTTPS REST API call to retrieve user profile data and related metadata`

**PlantUML:**
```plantuml
A --> B : HTTPS / REST\nGet profile
C ..> D : Message Queue\nOrder created
```

**Mermaid:**
```mermaid
A -->|HTTPS / REST - Get profile| B
C -.->|Message Queue - Order created| D
```

## 6. Diagram Types

### PlantUML

**Use Case Diagram** - Functional requirements, user goals

```plantuml
@startuml
left to right direction
skinparam packageStyle rectangle

actor Customer
actor Administrator

rectangle "E-Commerce System" {
  usecase (Browse Products) as UC1
  usecase (Place Order) as UC2
  usecase (Manage Inventory) as UC3
  usecase (Process Payment) as UC4
}

Customer --> UC1
Customer --> UC2
Administrator --> UC3
UC2 ..> UC4 : <<include>>
@enduml
```

**Use Case Diagram Best Practices: One Diagram Per Use Case**

When creating use case diagrams for requirements specifications, follow this approach:

- **One Diagram**: Each use case specification should have its own focused diagram
- **Include Only Relevant Actors**: Show only actors directly involved in that specific use case
- **System Boundary**: Use rectangle to define system boundary
- **Limit Scope**: Keep to 3-5 actors per diagram for clarity
- **Individual Focus**: Each diagram represents a single goal/scenario

**Example: Individual Use Case Diagram (UC-002)**
```plantuml
@startuml
left to right direction
skinparam packageStyle rectangle

actor Customer

rectangle "E-Commerce System" {
  usecase (Place Order) as UC2
  usecase (Process Payment) as UC4
}

Customer --> UC2
UC2 ..> UC4 : <<include>>
@enduml
```

**Data Flow Diagram** - Data movement through processes

```plantuml
@startuml
!define PROCESS rectangle
!define DATASTORE database
!define EXTERNAL component

EXTERNAL "User" as user
PROCESS "Process Order" as proc1
PROCESS "Validate Payment" as proc2
DATASTORE "Order DB" as db1
EXTERNAL "Payment Gateway" as gateway

user -> proc1 : Order details
proc1 -> proc2 : Payment info
proc2 -> gateway : Charge request
proc2 -> db1 : Store order
@enduml
```

**Component Diagram** - Internal structure and dependencies

```plantuml
@startuml
package "Application" {
  [UI Layer] --> [Service Layer]
  [Service Layer] --> [Data Layer]
  [Service Layer] --> [External API Client]
}

database "Database" as DB
cloud "External API" as API

[Data Layer] --> DB
[External API Client] ..> API
@enduml
```

**Sequence Diagram** - Time-ordered interactions

```plantuml
@startuml
actor User
participant "API Gateway" as API
participant "Auth Service" as Auth
participant "User Service" as User_Svc
database "User DB" as DB

User -> API : POST /login
API -> Auth : Validate credentials
Auth -> DB : Query user
DB --> Auth : User data
Auth --> API : JWT token
API --> User : 200 OK + token
@enduml
```

### Mermaid

**Graph** - System context, component relationships

```mermaid
graph LR
  A[User] --> B[API Gateway]
  B --> C[Auth Service]
  B --> D[User Service]
  C --> E[(Database)]
  D --> E
```

**Sequence Diagram** - Interaction flows, API sequences

```mermaid
sequenceDiagram
  participant User
  participant API
  participant Auth
  participant DB

  User->>API: POST /login
  API->>Auth: Validate credentials
  Auth->>DB: Query user
  DB-->>Auth: User data
  Auth-->>API: JWT token
  API-->>User: 200 OK
```

**C4 Context** - System boundaries, external dependencies

```mermaid
C4Context
  title System Context Diagram for E-Commerce Platform

  Person(customer, "Customer", "Users who purchase products")
  Person(admin, "Administrator", "Manages inventory and orders")

  System(ecommerce, "E-Commerce Platform", "Allows users to browse and purchase products")

  System_Ext(payment, "Payment Gateway", "Processes payments")
  System_Ext(shipping, "Shipping Provider", "Handles order delivery")

  Rel(customer, ecommerce, "Uses", "HTTPS")
  Rel(admin, ecommerce, "Manages", "HTTPS")
  Rel(ecommerce, payment, "Processes payments via", "HTTPS/REST")
  Rel(ecommerce, shipping, "Tracks shipments via", "HTTPS/REST")
```

**C4 Component** - Internal structure, technology stack

```mermaid
C4Component
  title Component Diagram for E-Commerce API

  Container_Boundary(api, "API Application") {
    Component(controller, "API Controllers", "ASP.NET Core", "Handles HTTP requests")
    Component(service, "Business Services", "C#", "Business logic")
    Component(repository, "Repositories", "Entity Framework", "Data access")
  }

  ContainerDb(db, "Database", "PostgreSQL", "Stores data")

  Rel(controller, service, "Uses")
  Rel(service, repository, "Uses")
  Rel(repository, db, "Reads/Writes", "SQL")
```

**Architecture (Beta)** - Deployment, infrastructure

```mermaid
architecture-beta
  group cloud(cloud)[Cloud Environment]

  service frontend(server)[Frontend] in cloud
  service api(server)[API Gateway] in cloud
  service backend(server)[Backend Services] in cloud
  service db(database)[Database] in cloud

  frontend:R --> L:api
  api:R --> L:backend
  backend:R --> L:db
```

## 7. Anti-Patterns

- **Spaghetti**: Too many crossing connectors; use hub nodes or split
- **God Component**: One element connected to all; decompose
- **Inconsistent Symbols**: Same role, different representations
- **Color Chaos**: >5 colors or no semantic meaning
- **Over-nesting**: >3 boundary levels
- **Kitchen Sink**: >10 elements; extract subviews

## 8. Usage Guide

| Scenario | Diagram | Tool |
|----------|---------|------|
| User interactions | Use Case | PlantUML |
| System boundaries | C4 Context | Mermaid |
| Internal components | Component | PlantUML/Mermaid |
| API flow | Sequence | PlantUML/Mermaid |
| Data movement | Data Flow | PlantUML |
| Infrastructure | Architecture | Mermaid |

## 9. References
- Refer `https://www.geeksforgeeks.org/system-design/use-case-diagram/`using websearch to understand the use case diagram and visual representations.
- Refer to `https://www.uml-diagrams.org/uml-25-diagrams.html` for detailed understanding of the various diagrams and visual representation. 
- Refer `https://docs.mermaidchart.com/icons/intro` on the usage of the icons.