
1. The SO ability just holds data. At runtime, a script needs to call GetWrapper() to actually get a useable ability.
2. To use an ability, call abilityWrapper.TryUse().
3. REMEMBER TO DISPOSE OF ABILITIES! The only way to upgrade an ability is to get a new wrapper. when re-assigning an abilityWrapper var, Dispose() of the old ability first!
4. Disposing will undo any active modifers.


5. For OnOff and Passive abilites, they need thier Use() functionality as per normal, and an "if(abilityWrapper.AbilityState==AbilityState.Deactivating){FinishAbility(); Return;}" gate clause.
6. Also override the Dispose() method to actually dispose of whatever modifiers were applied.


7. Active abilites are used when the button is clicked.
8. Channel abilites are active abilites that don't impliment OnAbilityFinished() in use, but rather after some duration in OnUpdate().
9. OnOff abilites are used once to turn on, and used again to turn off and trigger the cooldown.
10. Passive abilites are used when the abilityWrapper is constructed, and are turned off when the abilityWrapper is Disposed()


11. Ability system Upgrades can give Flat value increases (eg. +2m ability radius), but the modifier system can only handle percent increases (eg. +20% ability radius). This is unlikely to be refactored.
12. Any OnOff ability effect can be a passive, if the ability SO is marked as Passive.


13. Upgrades apply stats local to the ability. The ability effects then decide to use those local stats to buff themselves (eg. +10% damage to grenade ability) or buff the character (+10% all ability damage)
14. Abilities folder is empty because I have not needed any ability aside from the base.