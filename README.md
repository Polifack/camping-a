# WIP: #
## Attacking State ##
* Añadir movimiento hacia adiante no Attack state, que dependa do tempo de carga e dun valor base que terá cada movimiento de ataque distinto
* Permitir rotación no momento no que o ataque esté cargado, para poder redirigilo, esto faise añadidndo un animation Event. Referencia de como facelo: elden ring
* Añadir hitboxes distintas ou actualizar os datos dunha misma hitbox para os ataques, esto hai que ver como queda mellor

* ARREGLADO BUG: o crossfade final de salir do ataque 1 e iniciar o ataque 2 a veces falla debido ao animation event que hai colocado ao final do Punch1_R. Ese evento devólvete ao DefaultState, entonces dependendo de como suceda o crossfade, ao enganchar o ataque 2 como combo, este realízase no DefaultState, permitindo movimiento e tal. Pasaría o mismo co ataque 2 e os siguientes ataques do combo. Teño que mirar como poder solucionar esto. ARREGLADO: Metin un statemachinebehaviour no state de locomotion que transiciona sempre ao DefaultState. Esto deberia funcionar ben sempre, pero deixoo anotado para ter en conta que esta transición se fai polo animator.

## Climbing Stairs State ##
* BUG: por algún motivo, se non pulsas teclas de movimiento volves ao default statre e caes da escaleira. Acabo de darme conta de por que: porque ao non haber aind animación de escalar, e usarse a animacion idle, esta ten un evento de resetear o estado ao Default, polo que se cae da escaleira. Ao añadir a animacioón de escalada pertinente, esto debería arreglarse solo.

## Interacting State ##
* Empeza a funcionar, podeste sentar na nevera e cagar nela.
* está wip ainda (non funciona de todo), porque o play de anim chamase cada frame no BeforeCharacterUpdate, e eso tense que chamar unha sola vez.  O de mover e rotar funciona, habría que ver como se fai para salir do state. Definitivamente é un comenzo bo, pero tamen hai que mirar de arreglar a clase abstracta Action, que se pode mellorar esto fijo.

## Cola de inputs ##
* implementar unha cola de inputs para os ataques sobretodo, para non ter que facer click frame-perfect para activar o siguiente ataque do combo. Supoño que unha aproximación sería: con eventos de animacion, activar unha ventana na que se lee hasta 1 input e gardalo nunha cola ou nun buffer a secas, e despois ao acabar a anim, poñer un evento que ejecute o input que se gardou nesa cola ou buffer. De esta maneira, minimo 1 input se ejecutaria frame perfect.

## LedgeGrabbing State ##
* reimplementar o código que deixei comentado, copiando a base do climbing stairs state, pero arreglando a posición na que se pega á mesh. Añadir unha layermask para as colisions, para que non faga ledgrab na xente ou objetos indeseados

## Swimming State ##
* copialo do walkthrough do kinematic character controller
