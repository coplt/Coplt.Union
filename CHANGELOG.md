## 0.19.0
- Add Union2
  - use .net10 extension to create union

## 0.17.0
- Add
  - UnionSymbolAttribute now allow Enum

## 0.16.0
- Add
  - Default parameter support for record mode

## 0.15.0
- Breaking Changes
  - Record mode no longer generates record structures, only view structures
  - View structs now generate ref structs on runtimes that support ref structs

## 0.14.0
- Add record view Deconstruct

## 0.13.0
- Breaking Changes
  - Add Record mode
    - Rewrote layout generation

## 0.12.1
- Optimize compilation speed

## 0.12.0
- Breaking Changes
  - Variant getters now return references, no more variant setters

## 0.11.0
- Fix Bug

## 0.10.0
- Breaking Changes
  - Tags are now no longer in explicit assignment order, and are no longer 1 if the first item is void

## 0.9.0
- Breaking Changes
  - Switch to the source package, and change the access rights of related classes from public to internal

## 0.8.0
- Breaking Changes
  - Renamed from Sera.Union to Coplt.Union, as Sera is no longer maintained. Renaming the namespace is a breaking change
