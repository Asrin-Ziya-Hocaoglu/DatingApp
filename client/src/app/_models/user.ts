export interface User {
    username: string;
    token: string;
    photoUrl: string;
    knownAs: string;
    gender: string;
    
    
}

// //TypeScript
// let data: number | string = 42; // hem string hemde number olabilir

// data = "10";

// interface Car{
//     color:string;
//     model:string;
//     topSpeed?: number; // nullable
// }

// const car1: Car = {
//     color : 'blue',
//     model: 'BMW',

// }

// const car2 : Car = {
//     color:'red',
//     model:'Mercedes',
//     topSpeed:100
// }

// const multiply = (x: number,y:number)=> {
//      x*y;
// }