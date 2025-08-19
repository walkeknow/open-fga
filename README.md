# OpenFGA (in .NET)

## Intro

### What is Open FGA

Fine-Grained Authorization (FGA) implies the ability to grant specific users permission to perform certain actions in specific resources

OpenFGA is a Scalable Auth System which allows:

- to have relationships btw **users** and **objects**
- and make auth checks

- Details
  - Move authorization logic outside of application code, making it easier to write, change and audit.
  - Increase velocity by standardizing on a single authorization solution.
  - Centralize authorization decisions and audit logs making it simpler to comply with security and compliance requirements.
  - Help their products to move faster because it is simpler to evolve authorization policies.

### How is the data stored?

In a store!

Will contain all auth check data (on who can access what and what and when)

Ideally 1 store for 1 application

Data cannot be shared btw stores

You will need to create a store in OpenFGA before adding an [authorization model](https://openfga.dev/docs/concepts#what-is-an-authorization-model) and [relationship tuples](https://openfga.dev/docs/concepts#what-is-a-relationship-tuple) to it.

### What is an auth model?

Rows that actually dictate who can do what and when

Deserialize a json and feed it to fgaClient writeAuthModel() to add it to client.

### Type

A string that defines a class of objects with similar characteristics

### Relation

Strings defined within a type definition (reader, writer)

### Relation definition

Things that need to happen for a relationship to exist [user]

### Conditions

is a function composed of 1 or more parameters in an expression defined using the Google Common Expression Language (CEL)

### Relationship Tupleslationship Tuple

A triplet containing a user, object, and a relation (and a condition)

A tuple consisting of a user, an object, and a relation (and a condition).

### Direct and implied relationships

Direct relation

```yaml
model: |
  model
   schema 1.1
  type user
  type document
   relations
    define reader: [user] or editor
    define editor: [user]

tuples:
  - user: user:bob
  - relation: editor
  - object: document:doc1
```

relationship bob↔editor↔doc1 is direct. bob↔viewer>doc1 is implied

### Check request

call to the openfga api, checking if user has a relationship w an obj

### List Request

A request to find out what's related

OpenFGA relies on Relationship-Based Access Control, which allows developers to easily implement Role-Based Access Control.

Call ListUsers for list of users and ListObjects for list of objects

### Contextual Tuples

Contextual tuples are special relationship tuples that exist temporarily within a specific API request

Use when:

- When you want to avoid synchronizing data to OpenFGA
- When using information that's only available at runtime
- When a user has multiple relationships with the same object, and you want to specify which relationship to consider (like below)

```csharp
var body2 = new ClientWriteRequest()
{
    Writes =
    [
        new()
        {
            User = "user:anne",
            Relation = "editor",
            Object = "document:Z",
        }
    ],
};

var body3 = new ClientCheckRequest
{
    User = "user:anne",
    Relation = "reader",
    Object = "document:Z",
    ContextualTuples =
    [
        new()
        {
            User = "user:anne",
            Relation = "editor",
            Object = "document:Z"
        }
    ]
};
```

### Type bound public access

special syntax represented by `<type>:*` . Can only be used for users.

```csharp
{
  "user": "user:*",
  "relation": "editor",
  "object": "document:new-roadmap"
}
```

## Concepts

In [Role-Based Access Control](https://en.wikipedia.org/wiki/Role-based_access_control) (RBAC), permissions are assigned to users based on their role in a system. For example, a user needs the `editor` role to edit content.

In [Attribute-Based Access Control](https://en.wikipedia.org/wiki/Attribute-based_access_control) (ABAC), permissions are granted based on a set of attributes that a user or resource possesses. For example, a user assigned both `marketing` and `manager` attributes is entitled to publish and delete posts that have a `marketing` attribute.

Policy-Based Access Control (PBAC) is the ability to manage authorization policies in a centralized way that’s external to the application code. Most implementations of ABAC are also PBAC.

[Relationship-Based Access Control](https://en.wikipedia.org/wiki/Relationship-based_access_control) (ReBAC) enables user access rules to be conditional on relations that a given user has with a given object *and* that object's relationship with other objects

### Modeling

The model defines the static blueprint of your authorization system - it's like the schema that describes what relationships are _possible_ between users and objects.

In your file, the model section defines:

```yaml
model: |
  model
    schema 1.1
    
  type user

  type folder
    relations
      define parent: [folder]
      define owner : [user]
      define viewer: [user]
      define editor: [user]

      define can_edit : editor or owner or can_edit from parent
      define can_view : viewer or can_edit
```

Key characteristics of models:

- Immutable: Each change creates a new version
- Defines types: Like `user`, `folder`, `document`
- Defines possible relations: Like `owner`, `editor`, `viewer`, `can_edit`
- Defines authorization logic: How permissions are inherited and computed
- Rarely changes: Only when new features are added

### Tuples: The Actual Relationship Data

Relationship Tuples represent the dynamic facts about who actually has what relationships to which objects. These are the concrete assignments that make the model "come alive."

In your file:

```yaml
tuples:
  - user: user:anne
    object: folder:root
    relation: owner

  - user: folder:root
    object: document:welcome
    relation: parent

  - user: user:bob
    object: document:welcome
    relation: owner
```

Key characteristics of tuples:

- Dynamic data: Can be added/removed frequently
- Concrete facts: "Anne owns the root folder", "Bob owns the welcome document"
- Required for authorization: Without tuples, all checks return false
- Three parts: `user`, `relation`, `object`

### Tests: Validation and Verification

Tests verify that your model and tuples work together correctly by checking expected authorization outcomes.

In your file:

```yaml
tests:
  - name: Tests for basic example
    check:
      - user: user:anne
        object: document:welcome
        assertions:
          can_edit: true
          can_view: true
```

Key characteristics of tests:

- Validation: Ensure the model works as expected
- Different types: `check`, `list_objects`, `list_users`
- Assertions: Expected true/false results for permissions
- Development tool: Help catch bugs before deployment

### How They Work Together

1. Model says: "Folders can have owners, and owners can edit anything in folders they own"
2. Tuples say: "Anne owns the root folder, and the welcome document is in the root folder"
3. Tests verify: "Anne should be able to edit the welcome document" ✅

### Real-World Analogy

Think of it like a database system:

- Model = Database schema (table definitions, relationships)
- Tuples = Actual data rows in the tables
- Tests = Unit tests that verify queries return expected results

The model defines the _rules_, tuples provide the _facts_, and tests ensure everything works correctly together.
