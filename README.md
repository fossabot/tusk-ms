# Tusk demo/reference application

My template for new small projects in the world of microservices.

## Technologies

* .Net Core 3.0
* EF Core 3.0
* FluentValidation and .net core default validation disabled
* AutoMapper
* MediatR for CQRS
* Swashbuckle Swagger with xml documention enabled
* Serilog with default console output

Inspired from <https://github.com/JasonGT/NorthwindTraders>

## Remarks

I did not added a respository / UoW pattern because in my opinion its to heavy for microservices.

## TODOs

* Add business logic example and an example test
* Add token controller to make jwt api token

## References

* <https://medium.com/agilix/entity-framework-core-enums-ee0f8f4063f2>

## License

This project is licensed under the Apache License - see the [LICENSE](https://github.com/FJuette/tusk-ms/blob/master/LICENSE) file for details.
