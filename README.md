# Escape the First Lesson

A first-person educational escape room built in Unity 6 (URP). A computer-science
student falls asleep in the first lesson and must escape a dream version of the school
by solving puzzles that each teach a core CS concept.

This branch (`code-only`) contains just the C# source code, for reading and review.
The full Unity project (scenes, prefabs, art) lives in the main branch.

## Levels and concepts taught
- Level 0 - Classroom: binary numbers (toggle 8 switches to match a target value)
- Logic gates: a 2D drag-and-drop minigame (AND / OR / NOT)
- Computer lab: PC building (find and install CPU, GPU, RAM, PSU, MB, HDD)
- Networking: a 2D pipe-connection minigame linking computers
- Block coding: a 2D sequencing minigame to guide ARIA to an exit
- Final door: a phone-keypad cipher (letters to numbers)
- Ending: the student wakes up and the teacher asks one CS question (quiz)

## Folder structure (Assets/Scripts)
- `Core/` - GameManager, SoundManager, SaveSystem, SceneMusic (persistent systems)
- `Player/` - first-person controller, camera, interaction raycast, footsteps
- `Interaction/` - doors, switches, pickups, searchable furniture, props
- `Puzzles/` - binary puzzle, access panel, PC building, cipher lock
- `UI/` - main menu, pause menu, inventory, dialogue typewriter
- `Flow/` - classroom intro/state machine, ending quiz
- `Minigames/` - launchers that embed the 2D minigames into the 3D world
- `Debug/` - developer testing console
- `2DLogic/`, `MinigameFunctioning/`, `Cables/`, plus the `PuzzleManager2x2/4x4`
  files - the three 2D minigames (logic gates, network, block coding)

## Built with
Unity 6 (URP), DOTween for animation, TextMeshPro for UI, Unity Input System,
Unity UI Extensions for the minigame wires.
