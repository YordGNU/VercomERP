# Verificación de Acceso de Administrador Maestro (Omnipresencia)

Se ha analizado la implementación de la seguridad multi-entidad para asegurar que el usuario `master` tenga acceso global sin restricciones.

## Hallazgos Técnicos

### 1. Detección de Usuario Maestro
En `IEntidadProvider.cs`, la propiedad `IsMaster` está correctamente implementada:
```csharp
public bool IsMaster => _httpContextAccessor.HttpContext?.User?.Identity?.Name == "master";
```
Esto coincide con el nombre de usuario configurado en `SeedData.cs`.

### 2. Filtros Globales en `AppDbContext.cs`
La lógica de filtrado utiliza una expresión `OR` que prioriza el estado de `IsMaster`:
```csharp
// Expresión lógica simplificada:
// (context.IsMaster || entity.EntidadId == context.CurrentEntidadId)
```
**Observación Crítica:** He notado que el uso de `Expression.Constant(this)` en `OnModelCreating` puede ser problemático debido al almacenamiento en caché del modelo de EF Core. Si el modelo se cachea (comportamiento por defecto), los valores de las propiedades del contexto podrían no actualizarse correctamente para cada solicitud si no se referencian como miembros del contexto de forma que EF los reconozca como parámetros dinámicos.

### 3. Aislamiento de Sucursales
El filtro de sucursales también incluye la excepción para el maestro:
```csharp
filterBody = System.Linq.Expressions.Expression.OrElse(isMasterExpr, filterBody);
```

## Riesgos Identificados
- **Caché del Modelo:** Si EF Core cachea el `QueryFilter` con una referencia estática a la instancia del contexto que ejecutó `OnModelCreating`, el aislamiento fallará o se volverá inconsistente entre usuarios.
- **Operaciones de Escritura:** Aunque la lectura está protegida, el usuario Maestro debe tener cuidado al crear registros, ya que los controladores actualmente asignan el `EntidadId` de la sesión (el cual, para el maestro, es el de la primera entidad creada en el Seed).

## Acciones Recomendadas
1. **Refactorizar `AppDbContext`:** Asegurar que la referencia a `CurrentEntidadId` e `IsMaster` en los filtros sea reconocida por EF como propiedades de la instancia actual y no como constantes.
2. **Validar en Controladores Administrativos:** Asegurar que el Maestro pueda elegir a qué entidad asignar nuevos registros si fuera necesario (aunque su rol es principalmente de supervisión).
