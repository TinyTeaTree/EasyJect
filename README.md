# EasyJect_For_Unity
Dependency Injection based framework for Unity projects

Integration - Copy the DependencyInjection and Editor folder to youre Project. You may assign them different Assembly Definitions if you want them to compile seperatly.

Usage
1) Inject EasyJect Behaviours, Clouds, Signals, and Commands via the [Inject] Attribute like so
[Inject] GameManager GM {get; set;}
The injection will only work on an EasyJect InjectionConsumer, InjectionBehaviour, registered Cloud, or Command.

2) To Create an Injectable MonoBehaviour, just inherit from InjectionBehaviour like so
```C#
using EasyJect;

public class MainGameUI : InjectionBehaviour
{
  [Inject] GameManager GM {get; set;}
}
```
3) To create a Signal, Create a class that inherits from a Signal like so
```C#
public class OnEnemyAttackedSignal : Signal
{

}
```
*All signals can automatically be [Inject]ed to any Behavoiur/Cloud/Command.

4) Use a Signal injected by Invoking and Listening to it like so
```C#
public class EnemyManager : InjectionBehaviour
{
  [Inject] OnEnemyAttackedSignal OnEnemyAttacked {get; set;}
  
  public void Attack(){
    OnEnemyAttacked.Invoke();
  }
}

public class MainGameUI : InjectionBehaviour
{
  [Inject] GameManager GM {get; set;}
  [Inject] OnEnemyAttackedSignal OnEnemyAttacked {get; set;}
  
  void Start(){
    OnEnemyAttacked.AddListener(DisplayEnemyAttack);
  }
  
  void DisplayEnemyAttack(){
    //Show Stuff
  }
}
```
5) Dont block the Awake() function, only place dependency logic in the Start() and further, because the framework initializes on Awake()
```C#
public class MainGameUI : InjectionBehaviour
{
  [Inject] GameManager GM {get; set;}
  [Inject] OnEnemyAttackedSignal OnEnemyAttacked {get; set;}
  
  [SerializeField] Image _fadeImage;
  
  protected override void Awake(){
    base.Awake();
    
    _fadeImage.enable = false;
    
    //Do not place here code that uses Injections, because they might have not yet been Injected themselves, Instead use the Start()
  }
  
  void Start(){
    OnEnemyAttacked.AddListener(DisplayEnemyAttack);
  }
  
  void DisplayEnemyAttack(){
    //Show Stuff
  }
}
```
6) TODO: Explain Commands

7) TODO: Explain Clouds

8)
