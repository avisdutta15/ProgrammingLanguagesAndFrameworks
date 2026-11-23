# Java Classpath, Packages, and JAR — Step-by-Step Tutorial

> A hands-on guide that walks through `javac`, classpath, `javap`, packages, and JAR files using a single evolving example. Follow along in your terminal.

---

## Table of Contents

1. [Setup — The Source Files](#1-setup--the-source-files)
2. [Scenario 1 — Compile and Run (Happy Path)](#2-scenario-1--compile-and-run-happy-path)
3. [Scenario 2 — Output .class to a Different Folder](#3-scenario-2--output-class-to-a-different-folder)
4. [Scenario 3 — NoClassDefFoundError and Classpath Fix](#4-scenario-3--noclassdeffounderror-and-classpath-fix)
5. [Scenario 4 — Real-World: Compiling Against a Library](#5-scenario-4--real-world-compiling-against-a-library)
6. [Ways to Set the Classpath](#6-ways-to-set-the-classpath)
7. [javap — Inspecting .class Files](#7-javap--inspecting-class-files)
8. [Introduction to Packages](#8-introduction-to-packages)
9. [javap on Packages and Classpath](#9-javap-on-packages-and-classpath)
10. [Common Classpath Errors — Summary](#10-common-classpath-errors--summary)
11. [Introduction to JAR](#11-introduction-to-jar)

---

## 1. Setup — The Source Files

We'll use three files throughout this tutorial:

**Hello.java:**
```java
public class Hello {
    public String SayHello() {
        return "Hello";
    }
}
```

**World.java:**
```java
public class World {
    public String SayWold() {
        return "World";
    }
}
```

**_1_HelloWorld.java** (the main class):
```java
public class _1_HelloWorld {
    public static void main(String[] args) {
        String hello = new Hello().SayHello();
        String world = new World().SayWold();

        System.out.println(hello + " " + world);
    }
}
```

**Starting directory:**
```
Code/
├── _1_HelloWorld.java
├── Hello.java
└── World.java
```

---

## 2. Scenario 1 — Compile and Run (Happy Path)

Compile all three files. By default, `.class` files are created in the same directory:

```bash
javac _1_HelloWorld.java Hello.java World.java   # compiles all 3 files, outputs .class in current directory
```

**Directory after compilation:**
```
Code/
├── _1_HelloWorld.java
├── _1_HelloWorld.class    ← compiled
├── Hello.java
├── Hello.class            ← compiled
├── World.java
└── World.class            ← compiled
```

Run the main class:

```bash
java _1_HelloWorld   # runs the main() method inside _1_HelloWorld.class
```

**Output:**
```
Hello World
```

Everything works — all `.class` files are in the same directory, so the JVM finds them with no classpath configuration needed.

---

## 3. Scenario 2 — Output .class to a Different Folder

Now let's say we want `Hello.class` and `World.class` in a separate `libraries` folder (like a real project separating app code from library code).

**Step 1:** Delete the existing `Hello.class` and `World.class` from the current directory:

```bash
del Hello.class World.class   # remove old .class files from current directory
```

**Step 2:** Recompile Hello and World into a `libraries` folder using `-d`:

```bash
javac -d libraries Hello.java World.java   # -d = destination directory for .class output
```

This automatically creates the `libraries` folder and places the `.class` files there:

```
Code/
├── libraries/
│   ├── Hello.class        ← compiled here
│   └── World.class        ← compiled here
├── _1_HelloWorld.java
├── _1_HelloWorld.class    ← still here from earlier
├── Hello.java
└── World.java
```

---

## 4. Scenario 3 — NoClassDefFoundError and Classpath Fix

Now try to run `_1_HelloWorld` (its `.class` is still in the current directory):

```bash
java _1_HelloWorld   # ❌ JVM finds _1_HelloWorld.class but can't find Hello.class (moved to libraries/)
```

**Error:**
```
Exception in thread "main" java.lang.NoClassDefFoundError: Hello
        at _1_HelloWorld.main(_1_HelloWorld.java:6)
Caused by: java.lang.ClassNotFoundException: Hello
        at java.base/jdk.internal.loader.BuiltinClassLoader.loadClass(BuiltinClassLoader.java:580)
        at java.base/java.lang.ClassLoader.loadClass(ClassLoader.java:490)
        ... 1 more
```

**Why?** The JVM found `_1_HelloWorld.class` in the current directory, but when it tried to load `Hello` (which `_1_HelloWorld` depends on), it couldn't find it — because `Hello.class` is now in `.\libraries`, not in the current directory.

### First Fix Attempt — Add libraries to classpath:

```bash
java -cp .\libraries _1_HelloWorld   # ❌ tells JVM to look ONLY in .\libraries (forgets current dir)
```

**Error:**
```
Error: Could not find or load main class _1_HelloWorld
Caused by: java.lang.ClassNotFoundException: _1_HelloWorld
```

**Why?** When you specify `-cp`, the JVM ONLY looks in the paths you provide. We told it to look in `.\libraries` — but `_1_HelloWorld.class` is in the current directory (`.`), which is no longer on the classpath!

### Correct Fix — Include BOTH locations:

```bash
java -cp ".\libraries;." _1_HelloWorld   # .\libraries = find Hello/World; . = find _1_HelloWorld
```

**Output:**
```
Hello World
```

The classpath `.\libraries;.` tells the JVM:
1. Look in `.\libraries` for classes (finds `Hello.class`, `World.class`)
2. Look in `.` (current directory) for classes (finds `_1_HelloWorld.class`)

---

## 5. Scenario 4 — Real-World: Compiling Against a Library

In a real production scenario, you receive compiled libraries (`.class` or `.jar` files) and need to compile your own code against them. You don't have the source code for the libraries — only the compiled output.

**Let's simulate this.** Delete the source files for Hello and World, and also delete the old `_1_HelloWorld.class`:

```bash
del Hello.java World.java _1_HelloWorld.class   # simulate: only library .class files and app source remain
```

**What we have now:**
```
Code/
├── libraries/
│   ├── Hello.class        ← the "library" (no source code available)
│   └── World.class        ← the "library"
└── _1_HelloWorld.java     ← our application source
```

This is the real-world situation: you have a library (like a JAR from Maven) and your own source code that depends on it.

### Try to Compile Without Classpath:

```bash
javac _1_HelloWorld.java   # ❌ javac can't find Hello and World (not in current dir)
```

**Error:**
```
_1_HelloWorld.java:6: error: cannot find symbol
        String hello = new Hello().SayHello();
                           ^
  symbol:   class Hello
  location: class _1_HelloWorld
_1_HelloWorld.java:7: error: cannot find symbol
        String world = new World().SayWold();
                           ^
  symbol:   class World
  location: class _1_HelloWorld
2 errors
```

**Why?** `javac` can't find `Hello` and `World` — they're not in the current directory or on the classpath. The compiler needs to know where the dependencies are, just like the runtime does.

### Fix — Tell javac Where the Dependencies Are:

```bash
javac -cp .\libraries _1_HelloWorld.java   # -cp tells javac where to find Hello.class and World.class
```

**Compilation succeeds.** `javac` uses the classpath to find `Hello.class` and `World.class` in `.\libraries`, verifies that the methods exist, and compiles `_1_HelloWorld.java`.

**Directory after compilation:**
```
Code/
├── libraries/
│   ├── Hello.class
│   └── World.class
└── _1_HelloWorld.java
└── _1_HelloWorld.class    ← newly compiled
```

### Run the Application (Same as Scenario 3):

```bash
java -cp ".\libraries;." _1_HelloWorld   # both paths needed: libraries for deps, . for main class
```

**Output:**
```
Hello World
```

**Key takeaway:** Both `javac` and `java` need the classpath when dependencies are in a different location. The classpath is needed at **compile time** (to verify types/methods exist) and at **runtime** (to load the actual classes).

---

## 6. Ways to Set the Classpath

There are **3 ways** to set the classpath:

### Method 1: Command-Line Options (Per-Command — Recommended)

All three forms are equivalent. They apply to **one command only**:

```bash
java -cp ".\libraries;." _1_HelloWorld          # short form: -cp
java -classpath ".\libraries;." _1_HelloWorld   # medium form: -classpath (same as -cp)
java --class-path ".\libraries;." _1_HelloWorld # long form: --class-path (Java 9+, same as -cp)
```

The next command won't remember the classpath:

```bash
java -cp ".\libraries;." _1_HelloWorld    # ✓ works — classpath set for this command
java _1_HelloWorld                         # ❌ fails — no classpath, JVM only checks current dir
```

### Method 2: Session Environment Variable (Temporary — Current Terminal)

```bash
# Windows CMD
set CLASSPATH=.\libraries;.

# Windows PowerShell
$env:CLASSPATH = ".\libraries;."

# Linux / Mac
export CLASSPATH=./libraries:.
```

Applies to all commands in the current terminal session:

```bash
set CLASSPATH=.\libraries;.          # Windows CMD: set classpath for this terminal session
java _1_HelloWorld                    # ✓ works — reads CLASSPATH env var
java _1_HelloWorld                    # ✓ still works (same session)
javac _1_HelloWorld.java              # ✓ javac also reads CLASSPATH
# Close terminal → gone
```

**Note:** If you use `-cp` on a command, it **overrides** the `CLASSPATH` environment variable.

### Method 3: Permanent System Environment Variable

Set `CLASSPATH` permanently via system settings:

**Windows:**
1. Right-click "This PC" → Properties → Advanced System Settings
2. Environment Variables → New
3. Variable name: `CLASSPATH`
4. Variable value: `.\libraries;.`

**Linux/Mac** — add to `~/.bashrc` or `~/.zshrc`:
```bash
export CLASSPATH=./libraries:.
```

**⚠️ Not recommended** — causes conflicts between projects. Use `-cp` per command or let build tools manage it.

### Summary Table

| Method | Scope | Persists? |
|--------|-------|-----------|
| `-cp` / `-classpath` / `--class-path` | Single command | No |
| `set CLASSPATH` / `export CLASSPATH` | Current terminal session | Until terminal closes |
| System environment variable | All terminals | Yes (permanent) |

### Multiple Paths on Classpath

Use `;` (Windows) or `:` (Linux/Mac) to separate paths:

```bash
java -cp ".\libraries;.;.\other-libs" _1_HelloWorld   # Windows: semicolons separate paths
java -cp "./libraries:.:./other-libs" _1_HelloWorld   # Linux/Mac: colons separate paths
java -cp ".\libraries;.\jars\*" _1_HelloWorld         # wildcard: all JARs in .\jars\ folder
```

---

## 7. javap — Inspecting .class Files

`javap` is the Java class file disassembler. It shows the structure of a compiled `.class` file — methods, fields, and optionally the bytecode instructions.

### Basic Usage

```bash
javap _1_HelloWorld.class   # shows public methods and constructors of the compiled class
```

**Output:**
```
Compiled from "_1_HelloWorld.java"
public class _1_HelloWorld {
  public _1_HelloWorld();
  public static void main(java.lang.String[]);
}
```

### Inspect a Class in Another Directory

```bash
javap -cp .\libraries Hello   # -cp tells javap where to find Hello.class
```

**Output:**
```
Compiled from "Hello.java"
public class Hello {
  public Hello();
  public java.lang.String SayHello();
}
```

### Show Private Members

```bash
javap -p -cp .\libraries Hello   # -p = show private fields and methods too
```

### Show Bytecode

```bash
javap -c _1_HelloWorld.class   # -c = disassemble bytecode (shows JVM instructions)
```

**Output (partial):**
```
public static void main(java.lang.String[]);
    Code:
       0: new           #7    // class Hello
       3: dup
       4: invokespecial #9    // Method Hello."<init>":()V
       7: invokevirtual #10   // Method Hello.SayHello:()Ljava/lang/String;
      ...
```

### javap on Standard Library Classes

```bash
javap java.lang.String       # inspect Java's own String class
javap java.util.ArrayList    # inspect ArrayList structure
javap -p java.lang.Integer   # see private internals of Integer wrapper
```

---

## 8. Introduction to Packages

### What is a Package?

A package groups related classes under a namespace. It's declared with the `package` keyword at the top of the file:

```java
package libraries;  // this class belongs to the "libraries" package

public class Hello {
    public String SayHello() {
        return "Hello";
    }
}
```

### Making Hello and World Part of a Package

Let's add a `package libraries;` statement to both files:

**Hello.java:**
```java
package libraries;

public class Hello {
    public String SayHello() {
        return "Hello";
    }
}
```

**World.java:**
```java
package libraries;

public class World {
    public String SayWold() {
        return "World";
    }
}
```

**_1_HelloWorld.java** (must import from the package now):
```java
import libraries.Hello;
import libraries.World;

public class _1_HelloWorld {
    public static void main(String[] args) {
        String hello = new Hello().SayHello();
        String world = new World().SayWold();

        System.out.println(hello + " " + world);
    }
}
```

### Compiling — `javac` Does NOT Create Package Folders Automatically (Without `-d`)

```bash
javac Hello.java World.java   # ⚠️ compiles but puts .class in CURRENT dir, not libraries/ folder
```

This compiles successfully, but it places `Hello.class` and `World.class` in the **current directory** — NOT in a `libraries/` folder. `javac` without `-d` ignores the package structure for output placement.

If you then try to run, the JVM won't find them because it expects classes in `package libraries` to be inside a `libraries/` directory.

**You need `-d` to create the matching directory structure:**

```bash
javac -d . Hello.java World.java   # -d . = output to current dir, creating package folders automatically
```

This reads the `package libraries;` statement and creates:

```
Code/
├── libraries/
│   ├── Hello.class        ← created by javac -d matching the package
│   └── World.class
├── _1_HelloWorld.java
├── Hello.java
└── World.java
```

Or output to a specific folder:

```bash
javac -d out Hello.java World.java   # creates out/libraries/Hello.class and out/libraries/World.class
```

Creates `out/libraries/Hello.class` and `out/libraries/World.class`.

### Compile the Main File

Before compiling, you can verify the fully qualified class names using `javap`:

```bash
javap -cp . libraries.Hello   # use fully qualified name (package.Class) with -cp pointing to root
```

**Output:**
```
Compiled from "Hello.java"
public class libraries.Hello {
  public libraries.Hello();
  public java.lang.String SayHello();
}
```

Now compile the main file (it needs the `libraries` package on classpath):

```bash
javac -cp . _1_HelloWorld.java   # -cp . = look in current dir for packages (finds ./libraries/Hello.class)
```

### Run the Application

```bash
java -cp . _1_HelloWorld   # . includes current dir; JVM resolves libraries/ package folder from here
```

**Output:**
```
Hello World
```

Since we used `javac -d .`, the `libraries/` folder with the `.class` files is inside the current directory. The classpath `.` (current directory) is enough — the JVM follows the package hierarchy from there: `.` → `libraries/` → `Hello.class`.

### If Classes Are in a Separate Output Folder

```bash
javac -d out Hello.java World.java      # compile libs to out/libraries/*.class
javac -cp out _1_HelloWorld.java        # compile main, pointing to out for dependencies
java -cp "out;." _1_HelloWorld          # run: out has packages, . has _1_HelloWorld.class
```

---

## 9. javap on Packages and Classpath

When classes are in packages, use the fully qualified name with `javap`:

```bash
javap -cp . libraries.Hello        # inspect packaged class (dots = package separator)
javap -p -cp . libraries.Hello     # -p = include private members
javap -c -cp . libraries.Hello     # -c = show bytecode instructions
```

**Output:**
```
Compiled from "Hello.java"
public class libraries.Hello {
  public libraries.Hello();
  public java.lang.String SayHello();
}
```

**Key rule:** Use dots (`.`) for the class name (package separator), not slashes. The `-cp` points to the root directory that contains the package folder.

---

## 10. Common Classpath Errors — Summary

| Error | Cause | Fix |
|-------|-------|-----|
| `ClassNotFoundException: Hello` | `Hello.class` not on classpath | Add its directory to `-cp` |
| `NoClassDefFoundError: Hello` | Found at compile time, missing at runtime | Ensure same classpath at runtime |
| `Could not find or load main class` | Main class not on classpath, or used `.class`/`.\` | Use class name only, include `.` in `-cp` |
| `cannot find symbol` (compile error) | Dependency `.class` not on classpath during `javac` | Add `-cp path` to `javac` command |
| `ClassNotFoundException` with packages | Directory structure doesn't match `package` declaration | Use `javac -d` to auto-create structure |
| Wrong class version | Compiled with newer JDK, running on older JRE | Match compile target: `javac --release 11` |

### The Golden Rules

1. `javac` needs `-cp` to find dependencies at **compile time**
2. `java` needs `-cp` to find all classes at **runtime**
3. When you use `-cp`, the current directory (`.`) is NOT automatically included — add it explicitly
4. Package declaration must match directory structure
5. Use fully qualified class name to run: `java -cp out com.example.Main`

---

## 11. Introduction to JAR

### What is a JAR?

A JAR (Java ARchive) is a ZIP file that bundles `.class` files and resources into a single distributable file. Instead of shipping dozens of `.class` files, you ship one `.jar`.

### Why Use JAR?

In our example, we have `Hello.class` and `World.class` in the `libraries` folder. In a real project, a library might have hundreds of class files. A JAR bundles them into one file:

```
Without JAR:                          With JAR:
libraries/                            libraries.jar (single file)
├── Hello.class                         ├── Hello.class
├── World.class                         ├── World.class
├── Utils.class                         ├── Utils.class
└── ... (100 more files)                └── META-INF/MANIFEST.MF
```

### Creating a JAR from Our Example

**Step 1:** Make sure the `.class` files exist:

```bash
javac -d libraries Hello.java World.java   # compile to libraries/ folder
```

**Step 2:** Create a JAR:

```bash
jar cf libraries.jar -C libraries .   # c=create, f=filename, -C=change to dir, .=include all
```

| Flag | Meaning |
|------|---------|
| `c` | Create a new JAR |
| `f` | Output to file (next argument is filename) |
| `-C libraries .` | Change to `libraries` directory, include all files |

**Step 3:** Verify the JAR contents:

```bash
jar tf libraries.jar   # t=table of contents, f=filename — lists all files inside the JAR
```

**Output:**
```
META-INF/
META-INF/MANIFEST.MF
Hello.class
World.class
```

### Using the JAR as a Library

Now compile and run using the JAR instead of the folder:

```bash
javac -cp libraries.jar _1_HelloWorld.java       # compile against the JAR (javac looks inside it)
java -cp "libraries.jar;." _1_HelloWorld         # run: JAR has deps, . has main class
```

**Output:**
```
Hello World
```

The JAR works exactly like a directory of `.class` files — the JVM looks inside it automatically.

### Creating an Executable JAR

If you want to package everything (including `_1_HelloWorld`) into a runnable JAR:

```bash
javac -d out -cp libraries Hello.java World.java   # compile library classes to out/
javac -d out -cp out _1_HelloWorld.java             # compile main class to out/ (finds deps in out/)
jar cfe myapp.jar _1_HelloWorld -C out .            # c=create, f=file, e=entry point (main class)
java -jar myapp.jar                                 # -jar runs the JAR using its manifest Main-Class
```

**Output:**
```
Hello World
```

### The Manifest File (`META-INF/MANIFEST.MF`)

Every JAR contains a manifest file at `META-INF/MANIFEST.MF`. It's a plain-text metadata file that tells the JVM about the JAR.

#### Default Manifest (library JAR — no main class):

```
Manifest-Version: 1.0
Created-By: 21.0.1 (Oracle Corporation)

```

> **Note:** The manifest must end with a newline character. Missing the trailing newline can cause issues.

#### Executable JAR Manifest (has a main class):

When you use `jar cfe myapp.jar _1_HelloWorld ...`, it generates:

```
Manifest-Version: 1.0
Created-By: 21.0.1 (Oracle Corporation)
Main-Class: _1_HelloWorld

```

The `Main-Class` entry is what makes `java -jar myapp.jar` work — the JVM reads this to know which class to execute.

#### With Packages:

```
Manifest-Version: 1.0
Created-By: 21.0.1 (Oracle Corporation)
Main-Class: com.example.app.Main

```

#### Custom Manifest with Classpath (for JARs that depend on other JARs):

```
Manifest-Version: 1.0
Main-Class: _1_HelloWorld
Class-Path: lib/gson.jar lib/slf4j.jar

```

This lets you run `java -jar myapp.jar` even when `myapp.jar` depends on other JARs — the `Class-Path` header tells the JVM where to find them (relative to the JAR location).

#### Creating a JAR with a Custom Manifest

**Step 1:** Create a `manifest.txt` file:

```
Main-Class: _1_HelloWorld
Class-Path: lib/gson.jar

```

> **Important:** Each line must follow `key: value` format. The file must end with a blank newline.

**Step 2:** Create the JAR using `m` flag:

```bash
jar cfm myapp.jar manifest.txt -C out .   # m=manifest file, uses manifest.txt for MANIFEST.MF content
```

**Step 3:** Verify by extracting the manifest:

```bash
jar xf myapp.jar META-INF/MANIFEST.MF   # extract just the manifest
type META-INF\MANIFEST.MF               # Windows: view contents
```

#### Common Manifest Headers

| Header | Purpose | Example |
|--------|---------|---------|
| `Manifest-Version` | Manifest format version | `1.0` |
| `Main-Class` | Entry point for `java -jar` | `com.example.Main` |
| `Class-Path` | Dependencies (space-separated, relative paths) | `lib/gson.jar lib/utils.jar` |
| `Created-By` | JDK version that created the JAR | `21.0.1 (Oracle Corporation)` |
| `Implementation-Version` | Your app/library version | `2.1.0` |
| `Sealed` | Prevent other JARs from adding classes to your packages | `true` |

### With Packages — JAR Creation

If your classes use packages:

```bash
javac -d out Hello.java World.java           # creates out/libraries/Hello.class (matches package)
javac -d out -cp out _1_HelloWorld.java      # compile main; -cp out finds the package classes
jar cfe myapp.jar _1_HelloWorld -C out .     # bundle everything into executable JAR
java -jar myapp.jar                          # run using manifest entry point
```

### Inspecting a JAR

```bash
jar tf libraries.jar               # list all files inside the JAR
javap -cp libraries.jar Hello      # inspect a class inside the JAR (no extraction needed)
jar xf libraries.jar               # x=extract, f=filename — extracts all contents to current dir
```

### JAR Summary

| Command | Purpose |
|---------|---------|
| `jar cf lib.jar -C dir .` | Create a JAR from compiled classes |
| `jar cfe app.jar MainClass -C dir .` | Create executable JAR |
| `jar tf lib.jar` | List JAR contents |
| `jar xf lib.jar` | Extract JAR |
| `java -cp "lib.jar;." MainClass` | Run with JAR as library |
| `java -jar app.jar` | Run executable JAR |
| `javap -cp lib.jar ClassName` | Inspect class inside JAR |

---

## Quick Reference

| Command | Purpose |
|---------|---------|
| `javac File.java` | Compile (output in same directory) |
| `javac -d out File.java` | Compile to specific output directory |
| `javac -cp path File.java` | Compile with dependencies on classpath |
| `java ClassName` | Run a class (from current directory) |
| `java -cp "path;." ClassName` | Run with classpath |
| `java -jar app.jar` | Run executable JAR |
| `javap ClassName.class` | Show class structure |
| `javap -c ClassName.class` | Show bytecode |
| `javap -p -cp path ClassName` | Show all members from classpath |
| `jar cf lib.jar -C out .` | Create a JAR |
| `jar cfe app.jar Main -C out .` | Create executable JAR |
| `jar tf lib.jar` | List JAR contents |



References:
- https://www.youtube.com/watch?v=o_MouGNA1AE
- https://www.youtube.com/watch?v=zJPFwGs4q9o
