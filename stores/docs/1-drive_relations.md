# Example

## Plain language

Your feature description should include the objects, users and groups of users participating in the system. Sentences should look like this:

> A user {user} can perform action {action} to/on/in {object types} ... IF {conditions}

- A user can create a document in a drive if they are the owner of the drive.
- A user can create a folder in a drive if they are the owner of the drive.
- A user can create a document in a folder if they are the owner of the folder. The folder is the parent of the document.
- A user can create a folder in a folder if they are the owner of the folder. The existing folder is the parent of the new folder.
- A user can share a document with another user or an organization as either editor or viewer if they are an owner or editor of a document or if they are an owner of the folder/drive that is the parent of the document.
- A user can share a folder with another user or an organization as a viewer if they are an owner of the folder.
- A user can view a document if they are an owner, viewer or editor of the document or if they are a viewer or owner of the folder/drive that is the parent of the document.
- A user can edit a document if they are an owner or editor of the document or if they are an owner of the folder/drive that is the parent of the document.
- A user can change the owner of a document if they are an owner of the document.
- A user can change the owner of a folder if they are an owner of the folder.
- A user can be a member of an organization.
- A user can view a folder if they are the owner of the folder, or a viewer or owner of either the parent folder of the folder, or the parent drive of the folder.

## List Objects

> - A user {user} can perform action {action} ***to/on/in {object type}*** ... IF {conditions} (object)
> - ... IF {first noun} ***of*** a/the (second noun)

1. document
2. folder
3. drive
4. user
5. organization

## List Relations (Foreign keys and permissions)

> - any noun that is the {noun} of a "{noun} of a/an/the {type}" expression. These are typically the **Foreign Keys** in a database (eg. **`owner` of the drive)**
> - any verb or action that is the {action} of a "can {action} (in) a/an {type}" expression. These are typically the **permissions** for a type. `can_create` a document in a drive
>

### object:organization (Start with group type objects)

- member

### object:document

- can_create
- can_share
- can_edit
- can_view
- can_change_owner
- parent
- editor
- owner

### object:folder

- can_create
- can_share
- can_view
- can_change_owner
- owner
- parent
- viewer

### object:drive

- owner
- viewer

### object:user

## Testing

### Write Tuples

- user: user:alice
  relation: can_create
  object: document:doc1

- user: folder:folder1
  relation: parent
  object: document:doc1

### Write Assertions

- Alice can edit document:doc1
- Alice can view document:doc2
