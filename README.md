# Unity + ROS 2: Gemelo digital del brazo myCobot 280

Proyecto de Unity que se conecta a ROS 2 (Jazzy) mediante TCP para visualizar y mover un brazo robótico **Elephant Robotics myCobot 280** (modelo URDF importado). Unity publica y recibe `sensor_msgs/JointState`; el lado ROS 2 corre en un contenedor Docker.

> Repositorio complementario (lado ROS 2 / Docker): **[UnityDocker](https://github.com/DiegoMurilloP/UnityDocker)**

## Arquitectura

```
+------------------------+      TCP :10000      +-----------------------------+
|  Unity 2022.3 (host)   | <------------------> |  Docker: ROS 2 Jazzy        |
|  ROS-TCP-Connector     |   mensajes ROS 2     |  ros_tcp_endpoint           |
|  URDF-Importer         |                      |  mycobot_ros2               |
+------------------------+                      +-----------------------------+
```

## Requisitos

| Componente | Versión |
|---|---|
| Unity Editor | **2022.3.62f1** (LTS) |
| ROS 2 | Jazzy (dentro de Docker) |
| Docker | Docker Desktop / Engine reciente |
| Paquete Unity | `ROS-TCP-Connector` (Unity-Technologies) |
| Paquete Unity | `URDF-Importer` **v0.5.2** |
| Git | con acceso a GitHub (los paquetes se descargan por Git URL) |

## Estructura del repositorio

```
Assets/
  GameObjects/         Prefabs del robot (mycobot, cobot1)
  Resources/           ROSConnectionPrefab (IP/puerto de conexión)
  RosMessages/         Mensajes y servicios C# generados (MycobotInterfaces)
  Scenes/              SampleScene (escena principal)
  Scripts/
    JointStatePublisher.cs            Publica /joint_states desde Unity
    SourceDestinationPublisher_280.cs Suscribe /joint_states y mueve el robot
  UDRF/                URDF y mallas (.dae) del myCobot 280
Packages/              manifest.json (dependencias)
ProjectSettings/       Configuración del proyecto
```

## Puesta en marcha

### 1. Levantar el lado ROS 2 (Docker)

Sigue el README de [UnityDocker](https://github.com/DiegoMurilloP/UnityDocker). Resumen:

```bash
git clone https://github.com/DiegoMurilloP/UnityDocker.git
cd UnityDocker
docker build -t unity-ros2 .
docker run -it --rm -p 10000:10000 unity-ros2
# dentro del contenedor:
ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=0.0.0.0
```

### 2. Abrir el proyecto en Unity

```bash
git clone https://github.com/DiegoMurilloP/Practica2ROS2Unity.git
```

1. Abre **Unity Hub** → *Add* → selecciona la carpeta clonada.
2. Usa la versión **2022.3.62f1** (instálala desde el Hub si no la tienes).
3. Espera a que el Package Manager resuelva los paquetes (requiere internet y Git).
4. Abre la escena `Assets/Scenes/SampleScene.unity`.

### 3. Configurar la conexión

En el menú **Robotics → ROS Settings** (o en `Assets/Resources/ROSConnectionPrefab.prefab`):

| Campo | Valor por defecto | Nota |
|---|---|---|
| ROS IP Address | `127.0.0.1` | Cambiar si el contenedor está en otra máquina |
| ROS Port | `10000` | Debe coincidir con el puerto publicado en Docker |
| Protocol | ROS 2 | |

### 4. Ejecutar

Pulsa **Play** en Unity. El indicador de conexión de ROS-TCP-Connector (esquina superior izquierda de la vista Game) debe pasar a verde / conectado.

## Scripts principales

| Script | Función | Tópico |
|---|---|---|
| `JointStatePublisher` | Lee `ArticulationBody.jointPosition` (rad) y publica en cada `FixedUpdate` | `/joint_states` (publica) |
| `SourceDestinationPublisher_280` | Recibe `JointState`, convierte rad → grados y fija el `xDrive` de cada articulación | `/joint_states` (suscribe) |

Mapeo de articulaciones ROS → Unity (definido en `SourceDestinationPublisher_280`):

| Nombre en ROS | Nombre en Unity |
|---|---|
| `joint2_to_joint1` | `joint2` |
| `joint3_to_joint2` | `joint3` |
| `joint4_to_joint3` | `joint4` |
| `joint5_to_joint4` | `joint5` |
| `joint6_to_joint5` | `joint6` |
| `joint6output_to_joint6` | `joint6_flange` |

## Verificación

Desde otra terminal dentro del contenedor (`docker exec -it <id> bash`):

```bash
ros2 topic list
ros2 topic echo /joint_states
```

## Problemas comunes

| Síntoma | Causa probable | Solución |
|---|---|---|
| Unity no conecta | Puerto no publicado o endpoint apagado | `docker run -p 10000:10000` y arrancar `default_server_endpoint` |
| Conecta pero no llegan mensajes | Endpoint con `ROS_IP` en `127.0.0.1` dentro del contenedor | Usar `-p ROS_IP:=0.0.0.0` |
| El robot no se mueve | Nombres de joint no coinciden | Revisar la tabla de mapeo anterior |
| Errores de mensajes `Mycobot*` | Mensajes no regenerados | *Robotics → Generate ROS Messages* |
| Paquetes no resuelven | Sin Git o sin internet | Instalar Git y reabrir el proyecto |

## Convenciones de contribución

- Rama principal: `main`. Trabaja en ramas `feature/<tema>` o `fix/<tema>`.
- Commits en formato `tipo: descripción` (`feat`, `fix`, `docs`, `refactor`).
- **No subas** `Library/`, `Temp/`, `Logs/`, `UserSettings/` ni `obj/` (usa el `.gitignore` de Unity).
- Haz commit siempre de los archivos `.meta`.
- Fuerza el modo de texto: *Edit → Project Settings → Editor → Asset Serialization = Force Text*.

## Créditos

- [Unity Robotics Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub): ROS-TCP-Connector, URDF-Importer
- [ROS-TCP-Endpoint](https://github.com/Unity-Technologies/ROS-TCP-Endpoint) (rama `dev-ros2`)
- [Elephant Robotics mycobot_ros2](https://github.com/elephantrobotics/mycobot_ros2)

## Autor

Diego Murillo, [@DiegoMurilloP](https://github.com/DiegoMurilloP)
