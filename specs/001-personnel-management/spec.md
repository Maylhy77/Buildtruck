# Feature Specification: Gestión de Personal

**Feature Branch**: `docs/personnel-specify`

**Created**: 2026-09-28

**Status**: Draft

**Input**: User description: "Reconstruir la funcionalidad de Gestión de Personal de BuildTruck. WHO: El administrador de BuildTruck que necesita gestionar los registros del personal. WHAT: Permitir registrar, editar, listar y consultar el detalle del personal, tomando como referencia las cuatro historias documentadas en specs/personnel-reverse-engineering/spec.md. WHY: Mantener centralizada y actualizada la información del personal para facilitar su gestión."

**Referencias**:

- Referencia funcional (no modificada): `specs/personnel-reverse-engineering/spec.md` (Día 2, manual).
- Comportamiento verificado en pruebas existentes: `BuildTruckBackend.Tests/PersonnelTests.cs`
  (el personal pertenece a una obra existente; no se admite documento duplicado dentro de la
  misma obra; la consulta por obra devuelve solo personal activo — ver Brechas).

## Clarifications

### Session 2026-09-28

- Q: ¿Quién usa la funcionalidad? → A: Solo existen dos roles, Supervisor y Gerente. El
  Supervisor registra, edita, lista, consulta y descarga. El Gerente solo lista, consulta y
  descarga; no registra, edita ni elimina. No existe el rol Administrador (el término
  "administrador" de la solicitud original se sustituye por estos dos roles).
- Q: ¿El listado muestra solo personal activo o todo el personal? → A: Activos e inactivos,
  mostrando su estado. Difiere del comportamiento actual (ver Brechas).
- Q: ¿Se incluyen las descargas de la especificación manual? → A: Sí: listado en PDF/Excel y
  ficha del trabajador, dentro de las historias de consulta.
- Q: ¿Se incluye la eliminación? → A: No; queda registrada como necesidad futura.
- Q: ¿Qué reglas de validación siguen el DNI y el correo? → A: DNI de exactamente 8 dígitos;
  correo con formato básico `usuario@dominio.ext`.
- Q: ¿Qué formato tiene la descarga de la ficha del trabajador? → A: Solo PDF.
- Q: ¿Qué obras puede ver cada rol? → A: El Supervisor solo accede a sus obras asignadas; el
  Gerente ve todas las obras (solo consulta y descarga del personal). La creación de obras y
  la asignación de Supervisores quedan como contexto, fuera de alcance.
- Q: ¿Puede registrarse el mismo DNI en obras distintas? → A: Sí; el DNI es único dentro de
  una obra, pero puede repetirse entre obras distintas.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrar personal (Priority: P1)

El Supervisor registra a un nuevo miembro del personal para que quede asociado a una obra.

**Why this priority**: Sin registro no existe información que listar, consultar ni editar; es
la base de las demás historias.

**Independent Test**: Registrar como Supervisor un trabajador con datos válidos en una obra
existente y comprobar que queda almacenado y asociado a esa obra.

**Acceptance Scenarios**:

1. **Given** una obra existente y un Supervisor autenticado, **When** registra un trabajador con
   todos los datos obligatorios válidos, **Then** el sistema guarda el registro asociado a la
   obra y confirma el éxito. *(AC-01)*
2. **Given** un Supervisor autenticado, **When** intenta registrar un trabajador omitiendo uno o
   más datos obligatorios, **Then** el sistema no guarda el registro e indica los datos
   pendientes. *(AC-02)*
3. **Given** un Supervisor autenticado, **When** intenta registrar un trabajador con datos de
   formato inválido, **Then** el sistema no guarda el registro e indica los datos a
   corregir. *(AC-03)*
4. **Given** una obra que ya tiene un trabajador con un número de documento determinado,
   **When** el Supervisor intenta registrar otro trabajador con el mismo documento en esa obra,
   **Then** el sistema rechaza el registro indicando que ya existe. *(AC-04)*
5. **Given** una obra que no existe, **When** el Supervisor intenta registrar un trabajador
   asociado a ella, **Then** el sistema rechaza el registro. *(AC-05)*
6. **Given** un Gerente autenticado, **When** intenta registrar un trabajador, **Then** el
   sistema rechaza la operación por falta de permisos y no guarda ningún registro. *(AC-06)*
7. **Given** un Supervisor autenticado y una obra que no tiene asignada, **When** intenta
   registrar un trabajador en esa obra, **Then** el sistema rechaza la operación por falta de
   acceso y no guarda ningún registro. *(AC-20)*
8. **Given** un trabajador registrado con un DNI en una obra, **When** el Supervisor registra a
   un trabajador con el mismo DNI en otra obra que tiene asignada, **Then** el sistema guarda el
   registro en esa otra obra. *(AC-25)*

---

### User Story 2 - Editar información del personal (Priority: P2)

El Supervisor modifica los datos de un trabajador registrado para corregir errores o
mantenerlos actualizados.

**Why this priority**: Garantiza que la información centralizada se mantenga correcta en el
tiempo.

**Independent Test**: Editar como Supervisor un trabajador existente con datos válidos y
comprobar que la consulta posterior devuelve los datos actualizados.

**Acceptance Scenarios**:

1. **Given** un trabajador existente, **When** el Supervisor guarda cambios con datos válidos,
   **Then** el sistema actualiza la información y confirma el éxito. *(AC-07)*
2. **Given** un trabajador existente, **When** el Supervisor intenta guardar dejando datos
   obligatorios vacíos, **Then** el sistema no actualiza la información e indica los datos
   pendientes. *(AC-08)*
3. **Given** un trabajador existente, **When** el Supervisor intenta guardar con datos de
   formato inválido, **Then** el sistema no actualiza la información e indica los datos a
   corregir. *(AC-09)*
4. **Given** un trabajador que no existe, **When** el Supervisor intenta editarlo, **Then** el
   sistema informa que el trabajador no fue encontrado y no realiza cambios. *(AC-10)*
5. **Given** un Gerente autenticado, **When** intenta editar un trabajador, **Then** el sistema
   rechaza la operación por falta de permisos y no modifica la información. *(AC-11)*
6. **Given** un Supervisor autenticado y un trabajador de una obra que no tiene asignada,
   **When** intenta editarlo, **Then** el sistema rechaza la operación por falta de acceso y no
   modifica la información. *(AC-21)*

---

### User Story 3 - Listar y descargar el personal de una obra (Priority: P3)

El Supervisor o el Gerente consulta la lista del personal de una obra, activo e inactivo, con
su nombre, documento, rol y estado, y puede descargarla en PDF o Excel.

**Why this priority**: Permite localizar al personal, es el punto de acceso a su detalle y
facilita compartir la información.

**Independent Test**: Consultar como Gerente el personal de una obra con trabajadores activos
e inactivos, verificar los campos mostrados y descargar la lista en ambos formatos.

**Acceptance Scenarios**:

1. **Given** una obra con trabajadores activos e inactivos, **When** el Supervisor o el Gerente
   consulta su lista de personal, **Then** el sistema muestra todos los trabajadores con
   nombre, documento, rol y estado (activo/inactivo). *(AC-12)*
2. **Given** una obra sin personal registrado, **When** el Supervisor o el Gerente consulta la
   lista, **Then** el sistema indica que no existe personal registrado en la obra. *(AC-13)*
3. **Given** la lista de personal de una obra, **When** el Supervisor o el Gerente solicita
   descargarla en PDF, **Then** el sistema genera un archivo PDF con los datos de la
   lista. *(AC-14)*
4. **Given** la lista de personal de una obra, **When** el Supervisor o el Gerente solicita
   descargarla en Excel, **Then** el sistema genera un archivo Excel con los datos de la
   lista. *(AC-15)*
5. **Given** un Supervisor autenticado y una obra que no tiene asignada, **When** intenta
   consultar o descargar la lista de personal de esa obra, **Then** el sistema rechaza la
   operación por falta de acceso. *(AC-22)*
6. **Given** un Gerente autenticado y cualquier obra existente, **When** consulta o descarga la
   lista de personal de esa obra, **Then** el sistema la muestra o la genera sin requerir
   asignación a la obra. *(AC-23)*

---

### User Story 4 - Consultar y descargar el detalle de un trabajador (Priority: P4)

El Supervisor o el Gerente selecciona un trabajador de la lista para ver su ficha completa sin
modificarla y puede descargarla.

**Why this priority**: Aporta la información completa del trabajador una vez localizado en la
lista.

**Independent Test**: Consultar como Gerente un trabajador existente, verificar que se muestran
todos sus datos en modo solo lectura y descargar su ficha.

**Acceptance Scenarios**:

1. **Given** un trabajador existente, **When** el Supervisor o el Gerente consulta su detalle,
   **Then** el sistema muestra todos sus datos registrados en modo solo lectura, incluida la
   fotografía si existe. *(AC-16)*
2. **Given** un trabajador sin fotografía asociada, **When** se consulta su detalle, **Then** el
   sistema muestra los demás datos e indica que no tiene fotografía. *(AC-17)*
3. **Given** un trabajador que no existe, **When** se consulta su detalle, **Then** el sistema
   informa que el trabajador no fue encontrado. *(AC-18)*
4. **Given** el detalle de un trabajador existente, **When** el Supervisor o el Gerente solicita
   descargar la ficha, **Then** el sistema genera un archivo PDF con todos los datos del
   trabajador. *(AC-19)*
5. **Given** un Supervisor autenticado y un trabajador de una obra que no tiene asignada,
   **When** intenta consultar su detalle o descargar su ficha, **Then** el sistema rechaza la
   operación por falta de acceso. *(AC-24)*

---

### Edge Cases

- Registro con documento ya existente en la misma obra → rechazado (AC-04).
- Registro asociado a una obra inexistente → rechazado (AC-05).
- Gerente intenta registrar o editar → rechazado por permisos (AC-06, AC-11).
- Supervisor opera sobre una obra no asignada → rechazado por falta de acceso (AC-20, AC-21,
  AC-22, AC-24).
- Edición o consulta de un trabajador inexistente → "no encontrado" (AC-10, AC-18).
- Obra sin personal → mensaje de lista vacía (AC-13).
- Trabajador sin fotografía → detalle sin imagen (AC-17).
- Mismo documento en obras distintas → permitido (AC-25).
- **Acceso sin autenticación (aplica a las cuatro historias)**: **Given** un usuario que no ha
  iniciado sesión o presenta credenciales de autenticación inválidas, **When** intenta acceder a
  cualquiera de las operaciones de Gestión de Personal, **Then** el sistema rechaza la solicitud
  y no permite consultar, registrar, modificar ni descargar información del personal. *(AC-26)*

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE permitir al Supervisor registrar un trabajador asociado a una obra
  existente.
- **FR-002**: El sistema DEBE rechazar el registro o la edición cuando falten datos
  obligatorios o tengan un formato inválido, indicando qué datos corregir.
- **FR-003**: El sistema DEBE rechazar el registro de un trabajador cuyo número de documento ya
  exista en la misma obra, y DEBE permitir registrar el mismo número de documento en obras
  distintas.
- **FR-004**: El sistema DEBE rechazar el registro asociado a una obra inexistente.
- **FR-005**: El sistema DEBE permitir al Supervisor editar los datos de un trabajador
  existente.
- **FR-006**: El sistema DEBE permitir al Supervisor y al Gerente listar el personal de una
  obra, incluyendo trabajadores activos e inactivos, mostrando nombre, documento, rol y estado.
- **FR-007**: El sistema DEBE permitir al Supervisor y al Gerente consultar el detalle completo
  de un trabajador en modo solo lectura.
- **FR-008**: El sistema DEBE informar "no encontrado" al editar o consultar un trabajador
  inexistente.
- **FR-009**: El sistema DEBE restringir el registro y la edición al rol Supervisor y rechazar
  estas operaciones para el rol Gerente.
- **FR-010**: El sistema DEBE permitir al Supervisor y al Gerente descargar la lista del
  personal de una obra en PDF y en Excel.
- **FR-011**: El sistema DEBE permitir al Supervisor y al Gerente descargar en PDF la ficha de un
  trabajador con todos sus datos.
- **FR-012**: El sistema DEBE exigir autenticación para todas las operaciones de esta
  funcionalidad (verificado por AC-26).
- **FR-013**: El sistema DEBE limitar al Supervisor a las obras que tiene asignadas: solo en
  ellas puede registrar, editar, listar, consultar y descargar información del personal.
- **FR-014**: El sistema DEBE permitir al Gerente listar, consultar y descargar la información
  del personal de cualquier obra existente, sin requerir asignación.

Datos obligatorios del registro: se toman de la especificación manual (documento/DNI, nombres,
rol, correo). Reglas de formato: el DNI DEBE tener exactamente 8 dígitos numéricos y el correo
DEBE tener un formato básico válido (`usuario@dominio.ext`).

### Roles y permisos

| Operación | Supervisor | Gerente |
|---|---|---|
| Obras visibles | Solo las asignadas | Todas |
| Registrar | Sí (obras asignadas) | No |
| Editar | Sí (obras asignadas) | No |
| Listar | Sí (obras asignadas) | Sí |
| Consultar detalle | Sí (obras asignadas) | Sí |
| Descargar lista / ficha | Sí (obras asignadas) | Sí |

### Key Entities *(include if feature involves data)*

- **Trabajador (Personal)**: miembro del personal asociado a una obra. Atributos según la
  especificación manual: nombres, documento (DNI), fecha de ingreso, teléfono, correo, rol,
  estado (activo/inactivo) y fotografía.
- **Obra (Proyecto)**: obra a la que se asigna el personal; una obra puede tener varios
  trabajadores y tiene uno o más Supervisores asignados, lo que determina su acceso.

## Brechas entre comportamiento actual y requerido

- **GAP-01 — Listado de personal**: la prueba existente
  `PersonnelQueryService_ReturnsOnlyActivePersonnelByProject`
  (`BuildTruckBackend.Tests/PersonnelTests.cs`) verifica que la consulta por obra devuelve solo
  personal activo. Esta especificación exige mostrar activos e inactivos (FR-006, AC-12). La
  prueba y el código no se modifican en esta etapa; la resolución se abordará en
  `/speckit-plan` y `/speckit-tasks`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de los criterios AC-01 a AC-26 cuenta con al menos una prueba automatizada
  que lo referencia y pasa (Principio VII).
- **SC-002**: Un Supervisor completa el registro de un trabajador en menos de 2 minutos.
- **SC-003**: Tras guardar una edición, el 100 % de las consultas posteriores muestran los datos
  actualizados.
- **SC-004**: El 100 % de los intentos de registro con documento duplicado en la misma obra y
  de registro o edición por un Gerente son rechazados.
- **SC-007**: El 100 % de los intentos de un Supervisor de operar sobre obras no asignadas son
  rechazados.
- **SC-005**: La lista del personal de una obra y el detalle de un trabajador se muestran en menos
  de 2 segundos para obras de hasta 500 trabajadores.
- **SC-006**: El 100 % de las descargas solicitadas (lista en PDF, lista en Excel y ficha)
  generan un archivo con los mismos datos mostrados en pantalla.

## Assumptions

- El Supervisor y el Gerente ya están registrados y autenticados en la plataforma.
- La obra existe antes de registrar o consultar su personal.
- La creación de obras y la asignación de Supervisores a obras las realiza el Gerente en otra
  funcionalidad del sistema; esta especificación solo consume esa asignación.
- La unicidad del documento se exige dentro de una misma obra y no entre obras distintas
  (confirmado en clarificación; coincide con el comportamiento verificado en pruebas existentes).

## Out of Scope

- Retiro de registros de trabajadores que ya no sean relevantes: necesidad futura registrada.
  No se decide todavía entre eliminación lógica o física.
- Creación de obras y asignación de Supervisores (responsabilidad del Gerente, gestionada en
  otra funcionalidad; se usa aquí solo como contexto).
- Asistencia, cálculo de días trabajados y montos (presentes en el código/pruebas existentes).
- Notificaciones generadas al registrar personal (observadas en pruebas existentes, no solicitadas).
