# BuildTruck Sprint 1 backend web-service scenarios.
# This file documents BDD-style integration and acceptance scenarios for Auth + Users and Projects.

@sprint1 @auth-users
Feature: Auth and Users Web Services
  Como usuario de BuildTruck
  Quiero autenticarme y gestionar la informacion de mi perfil
  Para acceder de forma segura a las funcionalidades del sistema

  Scenario: Successful login with valid credentials
    Given que existe un usuario registrado con correo y password validos
    When el usuario envia una solicitud POST al endpoint de login
    Then el sistema debe retornar una respuesta exitosa
    And la respuesta debe incluir un token JWT
    And la respuesta debe incluir la informacion basica del usuario

  Scenario: View and update profile data
    Given que existe un usuario autenticado en el sistema
    When se actualizan los datos permitidos del perfil
    Then el sistema debe conservar la identidad del usuario
    And debe reflejar los nuevos datos personales
    And debe mantener las reglas de generacion de correo corporativo

@sprint1 @projects
Feature: Projects Web Services
  Como usuario de BuildTruck
  Quiero gestionar obras y proyectos
  Para administrar la informacion principal de cada obra de construccion

  Scenario: Create a project with valid data
    Given que el usuario tiene permisos para crear una obra
    When envia los datos requeridos del proyecto
    Then el sistema debe registrar la obra correctamente
    And la obra debe quedar disponible para su consulta

  Scenario: Validate invalid project data
    Given que el usuario intenta registrar o editar una obra
    When envia datos incompletos o invalidos
    Then el sistema debe rechazar la operacion
    And debe retornar los errores de validacion correspondientes

  Scenario: Verify project existence and user access
    Given que existe una obra registrada en BuildTruck
    And el usuario cuenta con un token JWT valido
    When se consulta la existencia de la obra
    Then el sistema debe confirmar que la obra existe
    And debe validar si el usuario tiene acceso al proyecto
