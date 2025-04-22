extends CharacterBody2D

@export var s_bullet_scene: PackedScene
@export var m_bullet_scene: PackedScene
@export var l_bullet_scene: PackedScene

func _ready():
	$AnimatedSprite2D.connect("animation_finished", _on_animation_finished)

func _on_animation_finished():
	if $AnimatedSprite2D.animation == "attack":
		$AnimatedSprite2D.play("idle")

func _process(delta: float) -> void:
	var facing_dir := -1 if $AnimatedSprite2D.flip_h else 1

	if Input.is_action_just_pressed("fire_small_wave"):
		fire_wave(s_bullet_scene, facing_dir)
	elif Input.is_action_just_pressed("fire_medium_wave"):
		fire_wave(m_bullet_scene, facing_dir)
	elif Input.is_action_just_pressed("fire_large_wave"):
		fire_wave(l_bullet_scene, facing_dir)


func fire_wave(bullet_scene: PackedScene, direction: int) -> void:
	if not bullet_scene:
		print("波场景未设置")
		return

	var bullet = bullet_scene.instantiate()
	# bullet.set_as_top_level(true)
	bullet.global_position = Vector2(-300, 80)

	# 如果波支持方向，可传入 direction（你需要在 bullet 脚本里加 direction 支持）
	#if bullet.has_variable("direction"):
	#	bullet.direction = direction

	get_tree().current_scene.add_child(bullet)

	$AnimatedSprite2D.play("attack")
