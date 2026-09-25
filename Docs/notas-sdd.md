# Notas sobre Spec-Driven Development (SDD)

Este documento resume el flujo de trabajo básico utilizando el agente Claude Code y Spec Kit.

## 1. Spec (/speckit-specify)
Es el equivalente a redactar una Historia de Usuario. Aquí le explicamos a la IA, en lenguaje natural, **qué** queremos construir o qué problema queremos resolver. Define los requerimientos y las reglas de negocio antes de tocar el código.

## 2. Plan (/speckit-plan)
Es el diseño técnico. La IA analiza nuestra especificación y nuestro código existente para proponernos **cómo** lo va a construir. Aquí define qué archivos modificará, qué patrones usará (como Clean Architecture) y cómo estructurará los controladores o repositorios. Nosotros aprobamos este plan antes de que empiece a programar.

## 3. Tareas (/speckit-tasks e implement)
Es la ejecución. La IA divide el plan técnico en tareas accionables y pequeñas. Al ejecutar la implementación, el agente escribe el código automáticamente siguiendo las reglas establecidas en la constitución del proyecto.
